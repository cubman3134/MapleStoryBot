#include <Wire.h>
#include <Keyboard.h>
// this is the keyboard unit
#define SLAVE_ADDR 9
#define ANSWERSIZE 10
void setup() {
  // put your setup code here, to run once:
  Wire.begin(SLAVE_ADDR);
  Wire.onReceive(receiveEvent);
  Serial.begin(9600);
  Keyboard.begin();
}

void receiveEvent() {
  byte curByte;
  char curChar;
  String curMessage = "";
  Serial.write("received data");
  while (Wire.available() > 0) 
  {
    curByte = Wire.read();
    curChar = (char)curByte;
    //Serial.write(curChar);
    curMessage += curChar;
  }
  String keyString;
  int keyStringInt;
  //if (curMessage.length() == ANSWERSIZE) 
  //{
      keyString = curMessage.substring(7);
      keyStringInt = keyString.toInt();
      char keyStringChar = keyStringInt;
      char charArray[keyString.length()];
      keyString.toCharArray(charArray, keyString.length());
      Serial.print(charArray);
      String curProcess = curMessage.substring(0, 7);
      char otherCharArray[curProcess.length() + 1];
      curProcess.toCharArray(otherCharArray, curProcess.length());
      //Serial.print(otherCharArray);
      if (curProcess == "KEYDOWN") 
      {
        Keyboard.press(keyStringChar);
      }
      else if (curProcess == "KEYLIFT")
      {
        Keyboard.release(keyStringChar);
      }    
  //}

}

void loop() {
  // put your main code here, to run repeatedly:

}
