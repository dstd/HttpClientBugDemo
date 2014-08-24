#!/usr/bin/python
import time
from BaseHTTPServer import BaseHTTPRequestHandler,HTTPServer

PORT_NUMBER = 8080

class myHandler(BaseHTTPRequestHandler):
	def do_POST(self):
		time.sleep(60);
		self.send_response(200)
		self.send_header('Content-type','text/html')
		self.end_headers()
		self.wfile.write("ok")
		return

try:
	server = HTTPServer(('', PORT_NUMBER), myHandler)
	server.serve_forever()

except KeyboardInterrupt:
	print '^C received, shutting down the web server'
	server.socket.close()
