#include <Wire.h>
#include <Mouse.h>
// this is the mouse unit
#define SLAVE_ADDR 10
#define ANSWERSIZE 10
void setup() {
  // put your setup code here, to run once:
  Wire.begin(SLAVE_ADDR);
  Wire.onReceive(receiveEvent);
  Serial.begin(9600);
}

void receiveEvent() {
  byte curByte;
  char curChar;
  String curMessage = "";
  while (Wire.available() > 0) 
  {
    curByte = Wire.read();
    curChar = (char)curByte;
    curMessage += curChar;
  }
  String xPosString, yPosString;
  int xPosInt, yPosInt;
  int startString;
  //if (curMessage.length() == ANSWERSIZE) 
  //{
      if (curMessage.substring(0, 2) == "MV") 
      {
        if (curMessage[2] == '0') {
          startString = 3;
          if (curMessage[3] == '0'){
            startString = 4;
            if (curMessage[4] == '0') {
              startString = 5;
            }
          }
        }
        else {
          startString = 2;
        }
        xPosString = curMessage.substring(startString, 6);
                if (curMessage[6] == '0') {
          startString = 7;
          if (curMessage[7] == '0'){
            startString = 8;
            if (curMessage[8] == '0') {
              startString = 9;
            }
          }
        }
        else {
          startString = 6;
        }
        yPosString = curMessage.substring(startString, 10);
        xPosInt = xPosString.toInt();
        yPosInt = yPosString.toInt();
        //char keyStringChar = keyStringInt;
        Mouse.move(xPosInt, yPosInt, 0);
        curMessage = "";
      }
      else if (curMessage == "MOUSELCLCK")
      {
        Serial.print("Left clicking mouse");
        Mouse.press(MOUSE_LEFT);
        curMessage = "";
      }
      else if (curMessage == "MOUSERCLCK")
      {
        Serial.print("Right clicking mouse");
        Mouse.press(MOUSE_RIGHT);
        curMessage = "";
      }
      else if (curMessage == "MOUSELRLAX")
      {
        Serial.print("Left releasing mouse");
        Mouse.release(MOUSE_LEFT);
        curMessage = "";
      }
      else if (curMessage == "MOUSERRLAX")
      {
        Serial.print("Right releasing mouse");
        Mouse.release(MOUSE_RIGHT);
        curMessage = "";
      }
  //}

}

void loop() {
  // put your main code here, to run repeatedly:

}
