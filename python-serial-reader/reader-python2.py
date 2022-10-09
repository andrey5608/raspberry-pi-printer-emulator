#!/usr/bin/env python
import time
import serial

ser = serial.Serial(
        port='/dev/serial0',
        baudrate = 9600
)

while 1:
        x=ser.readline()
        print "serial0:"
        print x
