                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                          #include <Wire.h>                                              
// this is the master unit
//SDA connects to Arduino Micro pin Labeled 2 (not A2) connect all 2                                                                                                                                                                                                                                                                                                       
//SCL connects to Arduino Micro, pin Labeled 3 (not A3) connect all 3
//GND connects to GND connect all 3 gnd pins
#define KEYBOARD_SLAVE_ADDR 9
#define MOUSE_SLAVE_ADDR 10
#define ANSWERSIZE 10
                                                                                                                                                                                                                                                                                                                                                          
String FullData;
void setup()
{
  Wire.begin();
  Serial.begin(9600);
  FullData = "";
}

char charData;
//int curCharInt;
void loop()
{
  if (Serial.available() > 0) 
  {
    charData = Serial.read();
    //Serial.print("got some data");
    //curCharInt = charData - '0';
    if (charData == '#') 
    { // 4 is end character of our serial messages
      // transfer string to slave arduinos
      Serial.print("key was equal to 4.");
      if (FullData[0] == 'M') 
      { // mouse message
        Wire.beginTransmission(MOUSE_SLAVE_ADDR);
        Serial.print("Writing to mouse.");
      }
      else 
      { // keyboard message
        Wire.beginTransmission(KEYBOARD_SLAVE_ADDR);
        Serial.print("Writing to keyboard.");
      }
      char charArray[FullData.length() + 1];
      FullData.toCharArray(charArray, FullData.length());
      Serial.print(charArray);
      Wire.write(charArray);
      Wire.endTransmission();
      FullData = "";
    }
    else 
    {
      FullData += charData;
      char charArray[FullData.length() + 1];
      FullData.toCharArray(charArray, FullData.length());
      Serial.print(charArray);
    }
  }
}
