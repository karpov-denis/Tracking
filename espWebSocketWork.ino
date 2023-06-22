#if !defined(ESP8266)
  #error This code is intended to run only on the ESP8266 boards ! Please check your Tools->Board setting.
#endif
const char* ssid = "";
const char* password = "";
#define _WEBSOCKETS_LOGLEVEL_     2
#define sensorPin 13
#include <ESP8266WiFi.h>
#include <ESP8266WiFiMulti.h>

#include <WebSocketsServer_Generic.h>

#include <Hash.h>

ESP8266WiFiMulti WiFiMulti;

WebSocketsServer webSocket = WebSocketsServer(80);
volatile bool IsPulce = false;
volatile bool Used = false;
volatile unsigned int StartTime;
volatile unsigned int prevPulce;
volatile int Length=0;
int pulceNumber=0; 
int curPos=0;
int positions[4];
void ICACHE_RAM_ATTR  GetEndOfPulse()
{
  if(!IsPulce){
  StartTime = millis(); 
  IsPulce = true;
  Used=false;
  }
  else
  {
  Length = millis()-StartTime;
  prevPulce = StartTime+Length;
  IsPulce=false;
  }
}

void NotifyAll()
{
  webSocket.broadcastTXT(String(positions[0])+"/"+String(positions[1])+"/"+String(positions[2])+"/"+String(positions[3]));
}

void webSocketEvent(const uint8_t& num, const WStype_t& type, uint8_t * payload, const size_t& length)
{
  (void) length;

  switch (type)
  {
    case WStype_DISCONNECTED:
      Serial.printf("[%u] Disconnected!\n", num);
      break;

    case WStype_CONNECTED:
    {
      IPAddress ip = webSocket.remoteIP(num);
      Serial.printf("[%u] Connected from %d.%d.%d.%d url: %s\n", num, ip[0], ip[1], ip[2], ip[3], payload);

      webSocket.sendTXT(num, "Connected");
    }
    break;

    case WStype_TEXT:
      Serial.printf("[%u] get Text: %s\n", num, payload);
      break;

    case WStype_BIN:
      Serial.printf("[%u] get binary length: %u\n", num, length);
      hexdump(payload, length);
      break;

    default:
      break;
  }
}

void setup()
{
  // Serial.begin(921600);
  Serial.begin(9600);
  attachInterrupt(digitalPinToInterrupt(sensorPin), GetEndOfPulse, CHANGE);
  Serial.print("\nStart ESP8266_WebSocketServer on ");
  Serial.println(ARDUINO_BOARD);
  Serial.println("Version " + String(WEBSOCKETS_GENERIC_VERSION));

  //Serial.setDebugOutput(true);

  WiFiMulti.addAP(ssid, password);

  //WiFi.disconnect();
  while (WiFiMulti.run() != WL_CONNECTED)
  {
    Serial.print(".");
    delay(100);
  }

  Serial.println();

  // print your board's IP address:
  Serial.print("WebSockets Server started @ IP Address: ");
  Serial.println(WiFi.localIP());

  webSocket.begin();
  webSocket.onEvent(webSocketEvent);
}
void loop() {
if(!IsPulce && !Used)
{
  if(Length>40&&Length<135)
    pulceNumber++; //получение синхронизирующих импульсов
  if(pulceNumber==2 && Length<50)
  {
    pulceNumber=0;
    positions[curPos++] = (millis() - prevPulce - 4000) * PI / 8333;
  }
  Used=true;
}
else if(curPos>4)
{
    NotifyAll();
}
  webSocket.loop();
}
