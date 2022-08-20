#include <SoftwareSerial.h>

#define ASCII_LF1 10
#define ASCII_NUL '\0'
#define ASCII_TAB '\t'  // Horizontal tab
#define ASCII_LF '\n'   // Line feed
#define ASCII_FF '\f'   // Form feed
#define ASCII_CR '\r'   // Carriage return
#define ASCII_EOT 4     // End of Transmission
#define ASCII_DLE 16    // Data Link Escape
#define ASCII_DC2 18    // Device control 2
#define ASCII_ESC 27    // Escape
#define ASCII_FS 28     // Field separator
#define ASCII_GS 29     // Group separator
#define ASCII_REPL 32   // Group separator

SoftwareSerial mySerial(10, -1);  // -1 if not using TX to save a pin

const byte numChars = 64; //64 is ok
char receivedChars[numChars];  // an array to store the received data
int receivedBytes[numChars];   // an array to store the received data

boolean newData = false;


void setup() {
  Serial.begin(115200);
  mySerial.begin(9600);
}

void loop() {
  recvWithEndMarker();

  if (newData) {
    showNewData();
    newData = false;
  }
}

void recvWithEndMarker() {
  static byte ndx = 0;
  char rc;
  int charNum;

  while (mySerial.available() > 0 && newData == false) {
    rc = mySerial.read();
    
    if (rc == 10 || (rc == 0 && receivedBytes[ndx - 1] == 61 && receivedBytes[ndx - 2] == 27)) {
      receivedChars[ndx] = '\0';  // terminate the string
      receivedBytes[ndx] = (int)rc;
      ndx = 0;
      newData = true;
      continue;
    }


    //Serial.write(rc);
    
    if (rc != ASCII_NUL) {
      receivedChars[ndx] = rc;
      receivedBytes[ndx] = (int)rc;
      ndx++;
      if (ndx >= numChars) {
        ndx = numChars - 1;
      }
    } else {
      receivedChars[ndx] = '\0';  // terminate the string
      receivedBytes[ndx] = (int)rc;
      ndx = 0;
      newData = true;
    }

    if ((receivedBytes[ndx] == 1 || receivedBytes[ndx] == 0) && receivedBytes[ndx - 1] == 61 && receivedBytes[ndx - 2] == 27) {
      receivedChars[ndx] = ASCII_REPL;
      receivedChars[ndx - 1] = ASCII_REPL;
      receivedChars[ndx - 2] = ASCII_REPL;
      ndx--;
      ndx--;
    } else {
      if (receivedBytes[ndx - 2] == 27) {
        receivedChars[ndx - 1] = ASCII_REPL;
      }
      if (receivedBytes[ndx - 1] == 27) {
        receivedChars[ndx] = ASCII_REPL;
        receivedChars[ndx - 1] = ASCII_REPL;
      }
    }
  }
}

void printArray() {
  String str;
  for (int i : receivedBytes) {
    str += " ";
    str += i;

    //Serial.print((char)i);
  }
  //Serial.println();
  Serial.println(str);
}

void showNewData() {
  //Serial.println("Received data:");
  Serial.println(receivedChars);
  //printArray(); // for chars debugging

}