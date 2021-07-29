#define pin13 13
#define pin11 11
#define pin9 9
#define pin7 7

int command = 0;

void setup() 
{
  //PIN 13
  pinMode(pin13, OUTPUT);
  digitalWrite(pin13, HIGH);
  
  //PIN 11
  pinMode(pin11, OUTPUT);
  digitalWrite(pin11, HIGH);
  
  //PIN 9
  pinMode(pin9, OUTPUT);
  digitalWrite(pin9, HIGH);
  
  //PIN 7
  pinMode(pin7, OUTPUT);
  digitalWrite(pin7, HIGH);

  //open serial comm
  Serial.begin(9600);
}

void loop() 
{
  if (Serial.available() > 0)
  {
      command = Serial.read();

      switch(command)
      {
        //AUX1
        case 100:
        {
            digitalWrite(pin13, HIGH);
            break;
        }
        case 101:
        {
            digitalWrite(pin13, LOW);
            break;
        }

        //AUX2
        case 200:
        {
            digitalWrite(pin11, HIGH);
            break;
        }
        case 201:
        {
            digitalWrite(pin11, LOW);
            break;
        }

        //AUX3
        case 300:
        {
            digitalWrite(pin9, HIGH);
            break;
        }
        case 301:
        {
            digitalWrite(pin9, LOW);
            break;
        }

        //AUX4
        case 400:
        {
            digitalWrite(pin7, HIGH);
            break;
        }
        case 401:
        {
            digitalWrite(pin7, LOW);
            break;
        }


        
      } 
  }
}
