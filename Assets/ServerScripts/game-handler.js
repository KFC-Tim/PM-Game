const WebSocket = require('ws');
const fs = require('fs');
const https = require('https');

// SSL certificate files
const sslOptions = {
    key: fs.readFileSync('/etc/letsencrypt/live/manager-rumble.de/privkey.pem'),
    cert: fs.readFileSync('/etc/letsencrypt/live/manager-rumble.de/fullchain.pem')
};

questionscount = 100;

// Read questions from JSON file
let questions = [];
fs.readFile('questions.json', 'utf8', (err, data) => {
    if (err) {
        console.error("Error reading questions file:", err);
        return;
    }
    questions = JSON.parse(data).questions;
    questionscount = questions.length;
});

// Create an HTTPS server
const httpsServer = https.createServer(sslOptions);

// Create WebSocket server and bind it to the HTTPS server
const wss = new WebSocket.Server({ server: httpsServer });

let games = {};
let players = {};

wss.on('connection', (ws) => {
    console.log("New client connected");

    ws.on('message', (message) => {
        try {
            let msg = JSON.parse(message);
            handleMessage(ws, msg);
        } catch (error) {
            console.error("Error parsing JSON:", error);
            ws.send(JSON.stringify({ type: 'error', message: 'Invalid JSON format' }));
        }
    });

    ws.on('close', () => {
        console.log("Client disconnected");
        removePlayerFromGames(ws);
    });
});

function handleMessage(ws, msg) {
    switch (msg.type) {
        case 'create':
            handleCreateGame(ws, msg.playerName);
            break;
        case 'join':
            handleJoinGame(ws, msg.playerName, msg.gameId);
            break;
        case 'start':
            handleStartGame(ws, msg.gameId);
            break;
        case 'answer':
            handleAnswer(ws, msg);
            break;
        default:
            console.warn("Unhandled message type:", msg.type);
            ws.send(JSON.stringify({ type: 'error', message: 'Unknown message type' }));
            break;
    }
}

function handleStartGame(ws, gameId) {
    if (!games[gameId]) {
        ws.send(JSON.stringify({ type: 'error', message: 'Game not found' }));
        return;
    }

    const game = games[gameId];
    if (game) {
        game.players.forEach(player => {
            players[player.uuid].ws.send(JSON.stringify({ type: 'start', message: 'Game started' }));
        });
    }

    console.log("Game started: " + gameId);
    gameMaster(gameId);
}

function handleCreateGame(ws, playerName) {
    const gameId = generateGameId();
    games[gameId] = {
        players: [],
        currentTurn: 0,
        scores: {},
        usedQuestions: []
    };
    handleJoinGame(ws, playerName, gameId);
    ws.send(JSON.stringify({ type: 'gameCreated', gameId }));
    console.log("Game created: " + gameId);
}

function handleJoinGame(ws, playerName, gameId) {
    if (!games[gameId]) {
        ws.send(JSON.stringify({ type: 'error', message: 'Game not found' }));
        return;
    }
    if (games[gameId].players.length >= 4) {
        ws.send(JSON.stringify({ type: 'error', message: 'Lobby is full' }));
        return;
    }
    const playerId = generatePlayerId();
    const startOffset = games[gameId].players.length * 10; // Set startOffset for the new player
    players[playerId] = { ws, playerName, gameId, startOffset, points: 0 }; // Initialize points for each player

    const playerPosition = games[gameId].players.length + 1;
    games[gameId].players.push({ uuid: playerId, name: playerName, number: playerPosition, startOffset });
    games[gameId].scores[playerId] = 0;

    ws.send(JSON.stringify({ type: 'joined', gameId, playerNumber: playerPosition }));

    // Broadcast update to all players in the game
    broadcastGameState(gameId);
    console.log("User joined: " + playerName + " ID: " + playerId + " as player number: " + playerPosition);
}

function broadcastGameState(gameId) {
    const game = games[gameId];
    if (game) {
        const state = {
            players: game.players,
            currentTurn: game.currentTurn,
            scores: game.scores
        };
        game.players.forEach(player => {
            players[player.uuid].ws.send(JSON.stringify({ type: 'update', state }));
        });
    }
}

function generateGameId() {
    const chars = '0123456789abcdefghijklmnopqrstuvwxyz';
    let id = '';
    for (let i = 0; i < 4; i++) {
        const randomIndex = Math.floor(Math.random() * chars.length);
        id += chars[randomIndex];
    }
    return id;
}

function generatePlayerId() {
    return 'player-' + Math.random().toString(9).substr(2, 9);
}

function removePlayerFromGames(ws) {
    const playerIdsToRemove = Object.keys(players).filter(playerId => players[playerId].ws === ws);
    playerIdsToRemove.forEach(playerId => {
        const { gameId } = players[playerId];
        const game = games[gameId];
        if (game) {
            // Remove player from game
            game.players = game.players.filter(player => player.uuid !== playerId);
            delete game.scores[playerId];

            // Check if there are no more players in the game
            if (game.players.length === 0) {
                // Delete the game
                delete games[gameId];
                console.log("Game deleted: " + gameId);
            }
        }
        delete players[playerId];
        // Broadcast updated game state after removing player
        if (game) {
            broadcastGameState(gameId);
        }
        console.log("User disconnected: " + "ID: " + playerId);
    });
}

function gameMaster(gameId) {
    const game = games[gameId];
    if (!game) {
        console.error("Game not found:", gameId);
        return;
    }

    const currentPlayer = game.players[game.currentTurn];
    if (!currentPlayer || !players[currentPlayer.uuid] || players[currentPlayer.uuid].ws.readyState !== WebSocket.OPEN) {
        game.currentTurn = (game.currentTurn + 1) % game.players.length;
        setTimeout(() => gameMaster(gameId), 100);
        return;
    }

    const currentPlayerId = currentPlayer.uuid;
    if (game.usedQuestions.length === questionscount) {
        game.usedQuestions = [];
    }

    let randomIndex;
    let question;
    do {
        randomIndex = Math.floor(Math.random() * questionscount);
        question = questions[randomIndex];
    } while (game.usedQuestions.includes(randomIndex));
    game.usedQuestions.push(randomIndex);

    const questionData = {
        question: question.questionText,
        answers: question.questionAnswers,
        topic: question.topic,
        points: question.steps
    };

    players[currentPlayerId].ws.send(JSON.stringify({
        type: 'question',
        questionData: questionData
    }));
    const previousPoints = players[currentPlayerId].points;
    players[currentPlayerId].ws.removeAllListeners('message');
    players[currentPlayerId].ws.once('message', (message) => {
        const parsedMessage = JSON.parse(message);
        if (parsedMessage.type === 'answer') {
            handleAnswer(players[currentPlayerId].ws, parsedMessage);

            // Check if the player has won
            if (players[currentPlayerId].points >= 120) {
                players[currentPlayerId].ws.send(JSON.stringify({
                    type: 'win',
                    message: 'Du hast das Spiel gewonnen!'
                }));

                // Remove the winner from the game
                game.players.splice(game.currentTurn, 1);

                // If only one player is left, end the game
                if (game.players.length === 1) {
                    const remainingPlayer = game.players[0];
                    players[remainingPlayer.uuid].ws.send(JSON.stringify({
                        type: 'game_over',
                        message: 'Das Spiel ist vorbei, du bist der letzte verbleibende Spieler.'
                    }));

                    // End the game
                    delete games[gameId];
                    return;
                } else {
                    // Update the game state after removing the winner
                    broadcastGameState(gameId);
                }
            } else {
                // Check if the player can kill another player by overtaking them or landing on their position
                const currentPoints = players[currentPlayerId].points;

                game.players.forEach(player => {
                    if (player.uuid !== currentPlayerId) {
                        const otherPlayerPoints = players[player.uuid].points;
                
                        // Calculate positions considering the initial offset
                        const currentPlayerPosition = (currentPoints + players[currentPlayerId].startOffset) % 40;
                        const previousPlayerPosition = (previousPoints + players[currentPlayerId].startOffset) % 40;
                        const otherPlayerPosition = (otherPlayerPoints + players[player.uuid].startOffset) % 40;
                
                        // Check if the current player has overtaken or landed on the other player
                        const hasLanded = currentPlayerPosition === otherPlayerPosition;
                        const hasOvertaken = previousPlayerPosition < otherPlayerPosition && currentPlayerPosition > otherPlayerPosition && currentPlayerPosition - otherPlayerPosition < 40;
                
                        if (hasLanded || hasOvertaken) {
                            let newPoints = 0;
                            if (otherPlayerPoints >= 80) {
                                newPoints = 80;
                            } else if (otherPlayerPoints >= 40) {
                                newPoints = 40;
                            }
                
                            players[player.uuid].points = newPoints;
                            games[gameId].scores[player.uuid] = newPoints;
                
                            players[player.uuid].ws.send(JSON.stringify({
                                type: 'killed',
                                message: `Du wurdest von ${players[currentPlayerId].name} geschlagen! Deine Punkte wurden zurückgesetzt zu ${newPoints}.`
                            }));
                
                            players[currentPlayerId].ws.send(JSON.stringify({
                                type: 'kill',
                                message: `Du hast ${players[player.uuid].name} geschlagen!`
                            }));
                        }
                    }
                });
                
                // Broadcast updated game state
                broadcastGameState(gameId);
            }

            // Move to the next turn only if no player won
            game.currentTurn = (game.currentTurn + 1) % game.players.length;
            setTimeout(() => gameMaster(gameId), 150);
        } else {
            console.warn("Unexpected message type:", parsedMessage.type);
        }
    });
}

function handleAnswer(ws, msg) {
    const playerId = Object.keys(players).find(id => players[id].ws === ws);
    if (!playerId) {
        console.error("Player not found for given WebSocket");
        return;
    }

    const gameId = players[playerId].gameId;
    const game = games[gameId];
    if (!game) {
        console.error("Game not found for player:", playerId);
        return;
    }

    const questionIndex = game.usedQuestions[game.usedQuestions.length - 1];
    const question = questions[questionIndex];

    const answer = msg.answer;
    const correctAnswer = question.correctAnswer;
    let points = 0;

    if (answer === correctAnswer) {
        points = question.steps;
        players[playerId].points = (players[playerId].points || 0) + points; // Ensure points property is initialized
        games[gameId].scores[playerId] += points;
    }

    // Broadcast the updated game state
    broadcastGameState(gameId);

    setTimeout(() => {
        gameMaster(gameId);
    }, 150);
}

// Start the HTTPS server
httpsServer.listen(8080, () => {
    console.log("----------------------------------------");
    console.log("Server is running on wss://manager-rumble.de:8080");
    console.log("----------------------------------------");
});
