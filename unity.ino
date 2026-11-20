
#include <ESP8266WiFi.h>
#include <ESP8266WebServer.h>
#include <DHT.h>

#include "secrets.h"

#define DHTPIN D2
#define DHTTYPE DHT11

#define SOIL_PIN A0
#define MQ135_PIN D6
#define LDR_PIN D7

DHT dht(DHTPIN, DHTTYPE);
ESP8266WebServer server(80);

// Replace with your calibrated readings
int SOIL_DRY_RAW = 850;
int SOIL_WET_RAW = 350;

const int ON_THRESHOLD = 35;
const int OFF_THRESHOLD = 45;

bool sprinklerOn = false;

float soilMoisture = 0;
float temperature = 0;
float humidity = 0;

int mq135Detected = 0;
int lightDetected = 0;

unsigned long lastRead = 0;

void readSensors()
{
    int soilRaw = analogRead(SOIL_PIN);

    soilMoisture = map(
        soilRaw,
        SOIL_DRY_RAW,
        SOIL_WET_RAW,
        0, 100
    );

    soilMoisture = constrain(soilMoisture, 0, 100);

    float t = dht.readTemperature();
    float h = dht.readHumidity();

    if (!isnan(t)) temperature = t;
    if (!isnan(h)) humidity = h;

    mq135Detected = digitalRead(MQ135_PIN);
    lightDetected = digitalRead(LDR_PIN);

    if (soilMoisture < ON_THRESHOLD)
        sprinklerOn = true;
    else if (soilMoisture > OFF_THRESHOLD)
        sprinklerOn = false;

    Serial.println("---------------------");
    Serial.print("Soil moisture: ");
    Serial.print(soilMoisture);
    Serial.println("%");

    Serial.print("Temperature: ");
    Serial.println(temperature);

    Serial.print("Humidity: ");
    Serial.println(humidity);

    Serial.print("Sprinkler: ");
    Serial.println(sprinklerOn ? "ON" : "OFF");
}

void handleData()
{
    String json = "{";

    json += "\"soil_moisture\":" + String(soilMoisture, 1) + ",";
    json += "\"temperature\":" + String(temperature, 1) + ",";
    json += "\"humidity\":" + String(humidity, 1) + ",";
    json += "\"mq135_detected\":" + String(mq135Detected) + ",";
    json += "\"light_detected\":" + String(lightDetected) + ",";
    json += "\"sprinkler_on\":";
    json += sprinklerOn ? "true" : "false";

    json += "}";

    server.send(200, "application/json", json);
}

void setup()
{
    Serial.begin(115200);

    pinMode(MQ135_PIN, INPUT);
    pinMode(LDR_PIN, INPUT);

    dht.begin();

    WiFi.mode(WIFI_STA);
    WiFi.begin(ssid, password);

    Serial.print("Connecting");

    while (WiFi.status() != WL_CONNECTED)
    {
        delay(500);
        Serial.print(".");
    }

    Serial.println("\nWiFi Connected!");

    Serial.print("IP Address: ");
    Serial.println(WiFi.localIP());
    delay(5000);
    readSensors();

    server.on("/data", HTTP_GET, handleData);
    server.begin();

    Serial.println("HTTP SERVER STARTED");
}

void loop()
{
    server.handleClient();

    if (millis() - lastRead >= 1000)
    {
        lastRead = millis();
        readSensors();
    }
}
