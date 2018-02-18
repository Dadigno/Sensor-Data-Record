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

void Decode(String command){
  if(command == "start"){
    SendValue();
  }else{
        Serial.println("Command Unknown");
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
      delay(100);
  }
}

