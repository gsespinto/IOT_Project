#include <toneAC.h>
#include "pitches.h"
#include <LiquidCrystal_I2C.h>

LiquidCrystal_I2C lcd(0x27, 16, 2);

#define PONT A0
#define BUTTON 2
#define LIGHT A1

#define MAX_FREQ 19

#define VREF 5.0f
#define STEPS 1024
#define R1 3.3f
#define TARGET_LUX 12.0f

#define DELAY 50

#define BUZZ_DURATION 250

// Serial notes that buzzer can play
int buzzerNotes[MAX_FREQ + 2] = { NOTE_B0, NOTE_D1, NOTE_F1, NOTE_GS1, NOTE_B1, NOTE_D2, NOTE_F2, NOTE_GS2, NOTE_B2, NOTE_D3, NOTE_F3, NOTE_GS3, NOTE_B3, NOTE_F4, NOTE_B4, NOTE_DS5, NOTE_FS5, NOTE_B5, NOTE_CS6, NOTE_F6, NOTE_A4};

String inputString = "";

int freqValue = 0;
String prefix = "";

void setup()
{
   // Initialize serial 
   Serial.begin(9600);
   Serial.setTimeout(DELAY);

   // Initialize LCD
   lcd.init();
   lcd.clear();         
   lcd.backlight();
  
   // Print static frequency label on first line of LCD
   lcd.setCursor(0,0);
   lcd.print("FREQUENCY: ");
   lcd.setCursor(1,0);

    // Sort pin modes
   pinMode(PONT, INPUT);
   pinMode(BUTTON, INPUT);
   pinMode(LIGHT, INPUT);

   // Store memory for input string
   inputString.reserve(200);
}
    
void loop() 
{
    // Get input from serial
    inputString = Serial.readString();

    // Handle inputs
    HandlePONT();
    HandleBUTT();
    HandleLIGHT();

    // Handle outputs
    HandleBUZZ(inputString);
    HandleLCD();
}

void HandlePONT()
{
    // Store mapped value of potentiometer
    freqValue = map(analogRead(PONT), 0, 1010, 0, MAX_FREQ);

    // Print it to serial
    Serial.print("FREQ ");
    Serial.println(freqValue);
}

void HandleBUTT()
{
  // Get current state of button
   byte value = digitalRead(BUTTON);

   // Print to serial
   Serial.print("BUTT ");
   Serial.println(value);
}

void HandleLIGHT()
{
   // Get photoresistor value
   int value = analogRead(LIGHT);

   // Coverted to lux
   float vin = (float)value * (VREF / STEPS);
   float lux = getLuxFromLDR(vin);

   // Print to serial whether the lux value
   // is above the target threshold or not
   Serial.print("LIGH ");
   if (lux >= TARGET_LUX)
      Serial.println(1);
   else
      Serial.println(0);
}

void HandleBUZZ(String input)
{  
  // Check if given input is to buzz
  if (input.length() < 6)
  {
    // Serial.println("Code too short!\n");
    return;
  }
    
  if (input.substring(0, 4) != "BUZZ")
  {
    // Serial.println("Wrong code!\n");
    return;
  }
    
  // Buzz buzzer to given tone
  toneAC(buzzerNotes[input.substring(5).toInt()], 10, BUZZ_DURATION);
  // Serial.println("Buzzed!");
}

void HandleLCD()
{
  // Set cursor to down line
  lcd.setCursor(0,1);  
  // Offset freq value by one
  int offsetedFreqValue = freqValue + 1;

  // Add prefix '0' if number is lower than 10
  if (offsetedFreqValue < 10 && prefix != "0")
    prefix = "0";
  else if (offsetedFreqValue >= 10 && prefix == "0")
    prefix = "";

  // Print current frequency to lcd
  lcd.print(prefix + offsetedFreqValue);
  lcd.print("              ");
}

float getLuxFromLDR(float vin){
  
  float _lux = ((2500/vin)-500)/R1; return(_lux);
}
