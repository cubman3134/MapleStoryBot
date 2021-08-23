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
  if (curMessage.length() == ANSWERSIZE) 
  {
      if (curMessage.substring(0, 1) == "MV") 
      {
        xPosString = curMessage.substring(2, 5);
        yPosString = curMessage.substring(6, 9);
        xPosInt = xPosString.toInt();
        yPosInt = yPosString.toInt();
        //char keyStringChar = keyStringInt;
        Mouse.move(xPosInt, yPosInt, 0);
      }
      else if (curMessage == "MOUSECLICK")
      {
        Mouse.press(MOUSE_LEFT);
      }
  }

}

void loop() {
  // put your main code here, to run repeatedly:

}
