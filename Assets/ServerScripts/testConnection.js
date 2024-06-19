const WebSocket = require('ws');

const socket = new WebSocket('wss://manager-rumble.de:8080/');

socket.on('open', function (event) {
    console.log('WebSocket is open now.');
    // You can send a message after the connection is opened
    socket.send(JSON.stringify({ type: 'create', playerName: 'TestPlayer' }));
});

socket.on('message', function (message) {
    console.log('WebSocket message received:', message.toString());
});

socket.on('close', function (event) {
    console.log('WebSocket is closed now.');
});

socket.on('error', function (error) {
    console.error('WebSocket error observed:', error);
});