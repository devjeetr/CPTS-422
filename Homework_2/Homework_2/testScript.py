
import socket

IP = '127.0.0.1'
port = 8080
BUFFER_SIZE = 1024

TEST_STRING = r"GET /home/ HTTP/1.1\r\nheader:value\r\nhi:worldzzzz\r\n\r\nTest String"

s = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
s.connect((IP,port ))
s.send(TEST_STRING)

data = s.recv(BUFFER_SIZE)

s.close()


print "Data: ", data

print len(data)


