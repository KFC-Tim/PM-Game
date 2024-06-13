const WebSocket = require('ws');
const fs = require('fs');

const questionscount = 2;

// Read questions from JSON file
let questions = [];
fs.readFile('questions.json', 'utf8', (err, data) => {
    if (err) {
        console.error("Error reading questions file:", err);
        return;
    }
    questions = JSON.parse(data).questions;
});

const wss = new WebSocket.Server({ port: 8080 });

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
        default:
            console.warn("Unhandled message type:", msg.type);
            break;
    }
}

function handleStartGame(ws, gameId) {
    if (!games[gameId]) {
        ws.send(JSON.stringify({ type: 'error', message: 'Game not found' }));
        return;
    }
    gameMaster(gameId);
}

function handleCreateGame(ws, playerName) {
    const gameId = generateGameId();
    games[gameId] = {
        players: [],
        currentTurn: 0,
        scores: {},
        usedQuestions: [] // Initialize usedQuestions array
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
    players[playerId] = { ws, playerName, gameId };
    const playerPosition = games[gameId].players.length + 1;
    games[gameId].players.push({ uuid: playerId, name: playerName, number: playerPosition });
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
    return 'game-' + Math.random().toString(36).substr(2, 9);
}

function generatePlayerId() {
    return 'player-' + Math.random().toString(36).substr(2, 9);
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

    // Check if the current player is still connected
    const currentPlayer = game.players[game.currentTurn];
    if (!currentPlayer || !players[currentPlayer.uuid] || players[currentPlayer.uuid].ws.readyState !== WebSocket.OPEN) {
        // If the current player is not connected, advance the turn
        game.currentTurn = (game.currentTurn + 1) % game.players.length;
        // Recursive call to handle the next player
        gameMaster(gameId);
        return;
    }

    const currentPlayerId = currentPlayer.uuid;
    const currentPlayerName = currentPlayer.name;

    // Check if all questions have been used
    if (game.usedQuestions.length === questionscount) {
        // Reset usedQuestions array
        game.usedQuestions = [];
    }

    // Get a random question that has not been used
    let randomIndex;
    let question;
    do {
        randomIndex = Math.floor(Math.random() * questionscount);
        question = questions[randomIndex];
    } while (game.usedQuestions.includes(randomIndex));
    // Mark the question as used
    game.usedQuestions.push(randomIndex);

    // Send the question to the current player
    const questionData = {
        question: question.questionText,
        answers: question.questionAnswers
    };

    players[currentPlayerId].ws.send(JSON.stringify({
        type: 'question',
        questionData: questionData
    }));

    // Handle response from the player
    players[currentPlayerId].ws.once('message', (message) => {
        // Check again if the player is still connected
        if (players[currentPlayerId].ws.readyState !== WebSocket.OPEN) {
            // If the player disconnected during their turn, skip their turn
            game.currentTurn = (game.currentTurn + 1) % game.players.length;
            broadcastGameState(gameId);
            setTimeout(() => {
                gameMaster(gameId);
            }, 300);
            return;
        }

        const answer = JSON.parse(message).answer;
        const correctAnswer = question.correctAnswer;
        let points = 0;

        // Check if the answer is correct
        if (answer === correctAnswer) {
            points = question.steps;
            games[gameId].scores[currentPlayerId] += points;
        }

        game.currentTurn = (game.currentTurn + 1) % game.players.length;
        broadcastGameState(gameId);

        setTimeout(() => {
            gameMaster(gameId);
        }, 300);
    });
}

console.log("----------------------------------------");
console.log("Server is running on ws://localhost:8080");
console.log("----------------------------------------");
