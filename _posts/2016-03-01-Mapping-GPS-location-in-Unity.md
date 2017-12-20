---
layout: post
title:  Map GPS location in Unity
category: technical
description: This technical blog shows how to map GPS location in Unity 
---
## Introduction
In order to map user location into Unity space, GPS is a must-have, because GPS is the only practical way to provide **accurate** location information. 
We plan to mount an arduino, a GPS module and a xbee transmitter on the headset, sending back user location in real-time.

![GPS module setup](https://lh3.googleusercontent.com/gEHNaGY-qwGzsatAbl9IRRfrOZxXD-uUrteexrynncgLkK_RKAEFEp5ZwWhYiSvxT3mq2TnLbt6E0q1AZ5g3FRfLT82RXICL97AUDr-VqI8OVb66KC9VFdFzq7TliZbOxGo_QcvRQzBsnTrA1HHjS2LwWpIWl5HNYFDhargKLuZDKwxTQ3MrzwLh0j1T40mKc6RoWO3ICjrYzrED0d93zGnekrsmcH9FbPOm7vUPOIqxB4rNxMLiZuXKj6wb_b3clj-OhUx00a6s8P6I0HSmRcHPhWraC171TR6hePGy7PgiEjrfOipKMMUZqQ4p3m2H9BuZWkfRFaIYoQk4Dza8eOiGPgjXvB7KeOYbk1l9un2sMFhgz0znSz6XS51IJR2jaVtqgKI9nqUlTDotMblGfeyW9ccbz8uhl1Vtj_i0CKFddvEOJe-y5TNNrzeDuTNua_ujAyH62fv-hwdp4MMtUsm7wfpCUAlCHeGTHuXBntPRuSfrIiQA_Kn0HAMK3rtZriuEq6EZjnPWU3aoMUHTbTrMMp5cpfy9XQMjgWYLoLkR7MLZHLlAjvJAiGw37D3rGMgqHNoUuRB9IPHu8_BqIL0STG_QVej8OldtpVRlR2EpcBF5ksO7_f4KXab8mwRBokXZen85hq_on4COVVGR_OGxSwy394trsA=w846-h634-no)

## Hardware Structure
![Hardware structure](https://lh5.googleusercontent.com/VuT3g2pa_Cf3QbSCCAPlfFce8l3rUeFRXKkAP0FOrTYMrpXvB_VAjCQUq9Am-58dhIHWWyH9RVyx51p_s049=w1546-h917)
As shown in the hardware structure, an arduino is used as information hub, connected to GPS, IMU and a XBEE, to send back collected information. As a minimum system, the GPS can be directly connected to the XBEE and in this case the XBEE is acting as wireless cable. However IMU(BNO055) has I2C interface and it requires extra configuration to be working, we placed an Arduino here.

### Q:Does Arduino have so many serial ports?
NO. Arduino UNO has only one hardware Serial port, which already been used as debug port connecting to a FT232 UART-USB bridge, but we can add more SoftwareSerial ports. I connect a SoftwareSerial port to GPS and connect another SoftwareSerial port to the XBEE. Arduino cannot write to two Serial ports at one time, but we only need to read data from GPS.

### Q: Can you provide arduino script?
```cpp 
#include <SoftwareSerial.h>
#include <TinyGPS.h>
#include <ArduinoJson.h>

/* This sample code demonstrates the normal use of a TinyGPS object.
   It requires the use of SoftwareSerial, and assumes that you have a
   4800-baud serial GPS device hooked up on pins 4(rx) and 3(tx).
*/

TinyGPS gps;

SoftwareSerial xbee(8, 9);
SoftwareSerial ss(10, 11);

#define BUFFER_SIZE 200

unsigned long timeStamp;

void setup()
{
  Serial.begin(115200);
  while (!Serial) {
    // wait serial port initialization
  }
  ss.begin(9600);
  while (!ss) {
    // wait serial port initialization
  }
  xbee.begin(9600);
  while (!xbee) {
    // wait serial port initialization
  }
  
  Serial.print("VRAR: GPS Test using TinyGPS ver."); Serial.println(TinyGPS::library_version());
  Serial.println();
}

void loop()
{
  ss.listen();
  bool newData = false;
  unsigned long chars;
  unsigned short sentences, failed;

  // For one second we parse GPS data and report some key values
  for (unsigned long start = millis(); millis() - start < 800;)
  {
    while (ss.available())
    {
      char c = ss.read();
      
       //Serial.write("LAA"); // uncomment this line if you want to see the GPS data flowing
      if (gps.encode(c)) // Did a new valid sentence come in?
        newData = true;
    }
  }

  timeStamp = millis();

  // Allocate a temporary memory pool
  DynamicJsonBuffer jsonBuffer(BUFFER_SIZE);
  JsonObject& root = jsonBuffer.createObject();

  root["DEVICE"] = "GPS.Regular.1";
  root["TIME_STAMP"] = timeStamp;
  root["STATUS"] = "IDLE";

  root["LAT"] = 0.0;
  root["LON"] = 0.0;
  root["ALT"] = 0.0;
  root["SAT"] = 0;
  root["PREC"] = 0;
  
  if (newData)
  {
    float flat, flon;
    unsigned long age;
    gps.f_get_position(&flat, &flon, &age);

    root["STATUS"] = "RUNNING";

    if (flat == TinyGPS::GPS_INVALID_F_ANGLE) {
      root["LAT"] = 0.0;
    } else {
      root["LAT"] = double_with_n_digits(flat, 6);
    }

    if (flon == TinyGPS::GPS_INVALID_F_ANGLE) {
      root["LON"] = 0.0;
    } else {
      root["LON"] = double_with_n_digits(flon, 6);
    }

    root["ALT"] = gps.altitude();

    if (gps.satellites() == TinyGPS::GPS_INVALID_SATELLITES) {
      root["SAT"] = 0;
    } else {
      root["SAT"] = gps.satellites();
    }

    if (gps.hdop() == TinyGPS::GPS_INVALID_HDOP) {
      root["PREC"] = 0;
    } else {
      root["PREC"] = gps.hdop();
    }

    gps.stats(&chars, &sentences, &failed);
    if (chars == 0){
      root["STATUS"]="FAILURE";
    }

    // Original Serial Outputs
    // Serial.print("LAT=");
    // Serial.print(flat == TinyGPS::GPS_INVALID_F_ANGLE ? 0.0 : flat, 6);
    // Serial.print(" LON=");
    // Serial.print(flon == TinyGPS::GPS_INVALID_F_ANGLE ? 0.0 : flon, 6);
    // Serial.print(" SAT=");
    // Serial.print(gps.satellites() == TinyGPS::GPS_INVALID_SATELLITES ? 0 : gps.satellites());
    // Serial.print(" PREC=");
    // Serial.print(gps.hdop() == TinyGPS::GPS_INVALID_HDOP ? 0 : gps.hdop());

    // root.printTo(Serial);
    // Serial.println();
  }else{
      root["STATUS"] = "IDLE";
      root["LAT"] = 0.0;
      root["LON"] = 0.0;
      root["ALT"] = 0.0;
      root["SAT"] = 0;
      root["PREC"] = 0;
  }

  // Original Status Outputs
  // Serial.print(" CHARS=");
  // Serial.print(chars);
  // Serial.print(" SENTENCES=");
  // Serial.print(sentences);
  // Serial.print(" CSUM ERR=");
  // Serial.println(failed);
  // Serial.println();

  // Print out nice JSON to Serial
  // root.prettyPrintTo(Serial);
  root.printTo(xbee);
  xbee.println();
  // Serial.print("Done");

}

```

### Q: Why use Json?
Parsing string can be very tricky especially when string format is not fixed yet. Using Json makes learning curve steeper, but it will bring benefits when scaling up the system. However I am lazy so this json message is not GeoJSON.

### Q: How do you map GPS location into Unity?
I pickup the first fixed location as initial location, and scale latitude/longitude movements into unity movements.
```cs 
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO.Ports;
using System.Text;




public class sp : MonoBehaviour
{
    #region GPS Variables
    public string DEVICE;
    public long TIME_STAMP;
    public string STATUS;
    public float LAT;
    public float LON;
    public float ALT;
    public int SAT;
    public int PREC;
    public float iniLAT;
    public float iniLON;
    public float iniALT;
    public bool firstGPSflag;

    private bool DEBUG = false;


    // private variables
    bool readDoneFlg = false;
    StringBuilder jsonBuffer = new StringBuilder();


    SerialPort streamCable = new SerialPort("COM4", 9600); //Set the port (com4) and the baud rate (9600, is standard on most devices)
    
    #endregion

    public float speed = 6.0F;
    public float jumpSpeed = 8.0F;
    public float gravity = 20.0F;
    private Vector3 moveDirection = Vector3.zero;

    public bool ccflag;

    // Use this for initialization
    void Start()
    {
        Debug.Log("Start  ");
        firstGPSflag = true;
        ccflag = true;
        //streamCable.DataReceived += new SerialDataReceivedEventHandler(StreamCable_DataReceived);
        streamCable.ReadTimeout = 100;
        streamCable.Open(); //Open the Serial Stream.
    }

    private void StreamCable_DataReceived(object sender, SerialDataReceivedEventArgs e)
    {
        if (readDoneFlg == true)
        {
            // wait for main program done with the current buffer
            return;
        }
        SerialPort sp = (SerialPort)sender;
        char indata = (char)sp.ReadChar();
        if (indata == '\n')
        {
            readDoneFlg = true;
        }else
        {
            if (DEBUG)
            {
                Debug.Log("read: " + indata);
            }
            jsonBuffer.Append(indata);
        }

    }

    // Update is called once per frame
    void Update()
    {
        if (!ccflag)
        {
            if (readGPS())
            {
                move();
            }
            if (Input.GetKeyDown(KeyCode.B))
            {
                ccflag = false;
                LAT = 0;
                LON = 0;
                iniLAT = 0;
                iniLON = 0;
            }
        }
        else
        {
            ccmove();
            if (Input.GetKeyDown(KeyCode.B))
            {
                ccflag = true;
            }
        }
    }

    bool readGPS()
    {
        
        if (!streamCable.IsOpen)
        {
            Debug.Log("Serial port it not open!");
        }
        char indata;
        while (true)
        {
            try
            {
                indata = (char)streamCable.ReadChar();
                if (indata == '\n')
                {
                    readDoneFlg = true;
                    if (DEBUG)
                    {
                        Debug.Log("read: " + jsonBuffer);
                    }
                   break;
                }
                else
                {  
                    jsonBuffer.Append(indata);
                }
            }
            catch (System.Exception)
            {
            }
        }
        
        
        if (readDoneFlg)
        {

            try
            {
                JsonUtility.FromJsonOverwrite(jsonBuffer.ToString(), this);
                jsonBuffer.Remove(0, jsonBuffer.Length);        // clear the buffer for the next reading
                readDoneFlg = false;
                if (DEBUG)
                {
                    Debug.Log("device = " + this.DEVICE);
                    Debug.Log("time_stamp = " + this.TIME_STAMP);
                    Debug.Log("status = " + this.STATUS);
                    Debug.Log("lat = " + this.LAT);
                    Debug.Log("lon = " + this.LON);
                    Debug.Log("alt = " + this.ALT);
                    Debug.Log("sat = " + this.SAT);
                    Debug.Log("prec = " + this.PREC);
                }
                    return true;
            }
            catch (System.Exception)
            {
                Debug.Log("fail to Json.");
                jsonBuffer.Remove(0, jsonBuffer.Length);
                readDoneFlg = false;
                throw;
            }
        }
        else
        {
            Debug.Log("No  ");
            return false;
        }
    }

    void ccmove()
    {
        CharacterController controller = GetComponent<CharacterController>();
        if (controller.isGrounded)
        {
            moveDirection = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
            moveDirection = transform.TransformDirection(moveDirection);
            moveDirection *= speed;
            if (Input.GetButton("Jump"))
                moveDirection.y = jumpSpeed;

        }
        moveDirection.y -= gravity * Time.deltaTime;
        controller.Move(moveDirection * Time.deltaTime);
    }

    void move()
    {
        if (firstGPSflag == true && LON != 0 && LAT != 0 && ALT != 0)
        {
            iniLON = LON;
            iniLAT = LAT;
            iniALT = ALT;
            firstGPSflag = false;
        }
        else if (LON != 0 && LAT != 0)
        {
            float la = (float)((LAT - 53.178469) / 0.00001 * 0.12179047095976932582726898256213);
            float lo = (float)((LON - 53.178469) / 0.00001 * 0.12179047095976932582726898256213);
            float ila = (float)((iniLAT - 53.178469) / 0.00001 * 0.12179047095976932582726898256213);
            float ilo = (float)((iniLON - 53.178469) / 0.00001 * 0.12179047095976932582726898256213);
            //transform.position = Quaternion.AngleAxis(LON-iniLON, -Vector3.up) * Quaternion.AngleAxis(LAT-iniLAT, -Vector3.right) * new Vector3(0, 0, 1);
            transform.position = new Vector3((lo-ilo), 0.5f, (la-ila));
        }
        
    }
}

```

___
## Test video

<iframe src="https://drive.google.com/file/d/1HbygO-TBlf4qhU_kncZNfbq__oM1iF276A/preview" width="640" height="480"></iframe>

