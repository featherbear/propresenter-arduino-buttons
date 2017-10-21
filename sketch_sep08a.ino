const byte ledPin = 17;

unsigned long lastDebounceTime = 0;  // the last time the output pin was toggled
unsigned long debounceDelay = 50;    // the debounce time; increase if the output flickers

void setup() {
  pinMode(ledPin, OUTPUT);
  pinMode(7, INPUT);
  pinMode(3, INPUT);
  pinMode(2, INPUT);
  pinMode(1, INPUT);
  pinMode(0, INPUT);
  Serial.begin(9600);
  Serial.print("Connected");
}

bool connected = false;
void completeConnect() {
  if (!connected) {
    attachInterrupt(digitalPinToInterrupt(7), goLogo, FALLING);
    attachInterrupt(digitalPinToInterrupt(3), goNext, FALLING);
    attachInterrupt(digitalPinToInterrupt(2), goPrev, FALLING);
    attachInterrupt(digitalPinToInterrupt(1), clearText, FALLING);
    attachInterrupt(digitalPinToInterrupt(0), clearAll, FALLING);
    connected = true;
  }

}

int t;
void loop() {
  if (Serial.available()) {
    t = Serial.read();
    if (t == 0xff) {
      completeConnect();
      Serial.print(":)");
    }
    else if (connected) {
      switch (t) {
        case 0xfe:
          if (connected) {
            updateLed();
            Serial.println(t, DEC);
          }
          break;
        default:
          Serial.print("Command not found?\n");
          break;
      }
    }
  }
}

/*
   Serial | Description | Key | Value
   -------|-------------|-----|------
     0    | Logo        | F6  | 0x75
     1    | Next        | ->  | 0x27
     2    | Prev        | <-  | 0x25
     3    | Text        | F2  | 0x71
     4    | All         | F1  | 0x70
   -------|-------------|-----|------
*/
void updateLed() {

}

int lastPress = -1;
void ChangeMode( byte mode ) {
  if (mode != lastPress) {
    lastPress = mode;
    Serial.print(mode);
  }
}
void goLogo() {
  ChangeMode(0x0);
}

void goNext() {
  ChangeMode(0x1);
}

void goPrev() {
  ChangeMode(0x2);
}


void clearText() {
  ChangeMode(0x3);
}

void clearAll() {
  ChangeMode(0x4);
}

