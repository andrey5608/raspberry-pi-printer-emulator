from escpos.printer import Network
# python-escpos

kitchen = Network("ip", 9100) # set Printer IP Address
kitchen.text("Hello World\n")
kitchen.cut()