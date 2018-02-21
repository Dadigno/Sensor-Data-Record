int _time = 0;
int samplingTime = 100;

void setup() {
  
  Serial.begin(9600);
  while(!Serial){}
  Serial.println("start");
}

void loop() {
  if(Serial.available() > 0){
    Decode(Serial.readString());
  }
}


  
void SetSampling(){
  Serial.println("Insert a new sampling time: ");
  while(1){
      if(Serial.available() > 0){
        _time = Serial.readString().toInt();
        if(_time){
          samplingTime = _time;
          Serial.println("Ok");
        }else{
          Serial.println("Value not valid");
        }
        return;
      }
      
  }
}

void SendValue(){
  while(1){
      if(Serial.available() > 0 && Serial.readString() == "stop"){
        return;
      }
      Serial.print(analogRead(A0));
      Serial.print(";");
      Serial.print(analogRead(A1));
      Serial.print(";");
      Serial.println(analogRead(A2));
      delay(samplingTime);
  }
}

void Decode(String command){
  if(command == "start"){
    SendValue();
  }else{
      if(command == "set"){
        SetSampling();
        }else{
        Serial.println("Command Unknown");
      }
    }
}
