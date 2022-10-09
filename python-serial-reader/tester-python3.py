#!/usr/bin/env python
# -*- coding: utf-8 -*-
import serial
test_string = "Test serial port ...".encode('utf-8')
port_list = ["/dev/serial0","/dev/serial1"]
while(True):
    for port in port_list:
        try:
            serialPort = serial.Serial(port, 9600, timeout = 10)
            print ("Serial port", port, " ready for test :")
            loopback = serialPort.read(1)
            print ("Received incorrect data:", loopback, "on serial part", port)
            serialPort.close()
        except IOError:
            print ("Error on", port,"\n")
