import http from "k6/http"

export let options = {
    vus: 1,
    duration: '10s'
}

export function setup(){
    http.patch("https://localhost:5001/Payload/current/100kb");
    // http.post("https://localhost:5001/WebSocket/connect");
}

export default function(){
    // let http20 = http.get("https://localhost:5001/Http/v20/send-receive");
    let http30 = http.get("https://localhost:5001/Http/v30/send-receive");
    // let ws = http.get("https://localhost:5001/WebSocket/send-receive");
    // let grpc = http.get("https://localhost:5001/Grpc/send-receive");

}

export function teardown(){
    // http.post("https://localhost:5001/WebSocket/disconnect");
}