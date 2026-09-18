package com.karlolegend.zrakoperka;

import android.content.Context;
import android.content.res.AssetManager;

import java.io.BufferedInputStream;
import java.io.BufferedOutputStream;
import java.io.IOException;
import java.io.InputStream;
import java.net.ServerSocket;
import java.net.Socket;
import java.nio.charset.StandardCharsets;
import java.util.HashMap;
import java.util.Locale;
import java.util.Map;

public final class LocalAssetServer {
    private final Context context;
    private ServerSocket serverSocket;
    private Thread serverThread;
    private volatile boolean running;
    private int port;

    public LocalAssetServer(Context context) {
        this.context = context.getApplicationContext();
    }

    public int start() throws IOException {
        serverSocket = new ServerSocket(0, 8);
        port = serverSocket.getLocalPort();
        running = true;
        serverThread = new Thread(this::runLoop, "ZRAKOPERKA-Offline-HTTP");
        serverThread.setDaemon(true);
        serverThread.start();
        return port;
    }

    public void stop() {
        running = false;
        try {
            if (serverSocket != null) serverSocket.close();
        } catch (IOException ignored) {}
    }

    private void runLoop() {
        while (running) {
            try {
                Socket socket = serverSocket.accept();
                Thread client = new Thread(() -> handle(socket), "ZRAKOPERKA-HTTP-Client");
                client.setDaemon(true);
                client.start();
            } catch (IOException e) {
                if (running) e.printStackTrace();
            }
        }
    }

    private void handle(Socket socket) {
        try (Socket client = socket;
             BufferedInputStream in = new BufferedInputStream(client.getInputStream());
             BufferedOutputStream out = new BufferedOutputStream(client.getOutputStream())) {

            String requestLine = readLine(in);
            if (requestLine == null || requestLine.isEmpty()) return;

            String[] parts = requestLine.split(" ");
            if (parts.length < 2) {
                sendText(out, 400, "Bad Request");
                return;
            }

            String method = parts[0];
            String rawPath = parts[1];
            while (true) {
                String header = readLine(in);
                if (header == null || header.isEmpty()) break;
            }

            if (!"GET".equals(method) && !"HEAD".equals(method)) {
                sendText(out, 405, "Method Not Allowed");
                return;
            }

            String path = rawPath.split("\\?", 2)[0];
            if (path.equals("/")) path = "/index.html";
            path = path.replace("..", "");
            if (path.startsWith("/")) path = path.substring(1);

            String assetPath = "www/" + path;
            AssetManager assets = context.getAssets();

            try (InputStream asset = assets.open(assetPath, AssetManager.ACCESS_STREAMING)) {
                int length = asset.available();
                Map<String, String> headers = new HashMap<>();
                headers.put("Content-Type", contentType(path));
                headers.put("Cache-Control", "no-store");
                headers.put("Cross-Origin-Opener-Policy", "same-origin");
                headers.put("Cross-Origin-Embedder-Policy", "require-corp");
                headers.put("Cross-Origin-Resource-Policy", "same-origin");
                headers.put("Content-Length", Integer.toString(length));
                writeHeaders(out, 200, "OK", headers);

                if ("GET".equals(method)) {
                    byte[] buffer = new byte[64 * 1024];
                    int read;
                    while ((read = asset.read(buffer)) != -1) {
                        out.write(buffer, 0, read);
                    }
                }
                out.flush();
            } catch (IOException missing) {
                sendText(out, 404, "Not Found: " + path);
            }
        } catch (IOException ignored) {}
    }

    private static String readLine(InputStream in) throws IOException {
        StringBuilder sb = new StringBuilder();
        int c;
        while ((c = in.read()) != -1) {
            if (c == '\n') break;
            if (c != '\r') sb.append((char)c);
            if (sb.length() > 8192) break;
        }
        return sb.toString();
    }

    private static void writeHeaders(BufferedOutputStream out, int code, String message, Map<String, String> headers) throws IOException {
        StringBuilder sb = new StringBuilder();
        sb.append("HTTP/1.1 ").append(code).append(' ').append(message).append("\r\n");
        for (Map.Entry<String, String> entry : headers.entrySet()) {
            sb.append(entry.getKey()).append(": ").append(entry.getValue()).append("\r\n");
        }
        sb.append("Connection: close\r\n\r\n");
        out.write(sb.toString().getBytes(StandardCharsets.US_ASCII));
    }

    private static void sendText(BufferedOutputStream out, int code, String text) throws IOException {
        byte[] body = text.getBytes(StandardCharsets.UTF_8);
        Map<String, String> headers = new HashMap<>();
        headers.put("Content-Type", "text/plain; charset=utf-8");
        headers.put("Content-Length", Integer.toString(body.length));
        writeHeaders(out, code, text, headers);
        out.write(body);
        out.flush();
    }

    private static String contentType(String path) {
        String p = path.toLowerCase(Locale.ROOT);
        if (p.endsWith(".html")) return "text/html; charset=utf-8";
        if (p.endsWith(".js")) return "application/javascript";
        if (p.endsWith(".wasm")) return "application/wasm";
        if (p.endsWith(".json")) return "application/json";
        if (p.endsWith(".css")) return "text/css";
        if (p.endsWith(".png")) return "image/png";
        if (p.endsWith(".jpg") || p.endsWith(".jpeg")) return "image/jpeg";
        if (p.endsWith(".svg")) return "image/svg+xml";
        if (p.endsWith(".ico")) return "image/x-icon";
        if (p.endsWith(".data")) return "application/octet-stream";
        return "application/octet-stream";
    }
}
