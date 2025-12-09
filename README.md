# QR Code Generator

A Minimal API and a Minimal Web Site that exposes an endpoint to generate QR Codes.

## Overview

QR Code Generator is a lightweight ASP.NET Core application that provides both a web interface and a RESTful API for generating QR codes. The application allows users to encode URLs, text, and other information into QR codes with customizable options including size and border settings.

### Key Features

- **Simple Web Interface**: User-friendly web UI for quick QR code generation
- **RESTful API**: `/qrcode` endpoint for programmatic QR code generation
- **Customizable Options**: Configure QR code size and border display
- **Rate Limiting**: Built-in protection with 50 requests per minute per IP
- **Output Caching**: Optimized performance with 1-hour cache duration
- **Download Support**: Easily download generated QR codes as PNG images

## Screenshot

![QR Code Generator Interface](https://github.com/user-attachments/assets/99ffbd21-8453-43ff-88ab-3479c8f43a8c)

## Try It Live

Experience the QR Code Generator in action: **[QR Code Assistant Live Demo](https://qrcodeassistant.azurewebsites.net)**
