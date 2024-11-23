#include <Wire.h>  // Для работы с I2C, я про неё подобробнее напишу в другом файле 
#include <MPU6050.h>  // Для упрощённого управления гироскопом MPU6050, без неё мы бы сдохли

MPU6050 mpu;  // Тут создается объект для работы с нашим модулем GY-521 (в нём и стоит чип MPU-6050)

const int numReadings = 10;   // Задаем размер буфера для усреднения данных, это значит, что ток последние 10 значений будут юзаться для вычисления среднего
float axBuffer[numReadings], ayBuffer[numReadings];  // Хуячим буферы для осей X и Y думаю тут пока ещё всё понятно
int bufferIndex = 0;  // Индекс для записи новых значений в буферы

void setup() {  // Я до сих пор не понял Faceless Void или Void Spirit, но похуй, опустим 
  Serial.begin(9600);  // Инит послед. порта для отладки
  Wire.begin();  // Инит I2C  
  mpu.initialize();  // Инит MPU6050

  if (mpu.testConnection()) {  // Требовалось в ходе разработки, для тестов, можно снести к хуям в итоговом варианте
    Serial.println("MPU6050 подключен.");  // Короче, если були 1(True) при методе тестирования подкл., то всё
  } else {
    Serial.println("Ошибка подключения MPU6050.");  // Если були 0(false), то ошибка покдлючения в цикле, пока 1(True) не будет
    while (1);
  }

  for (int i = 0; i < numReadings; i++) {
    axBuffer[i] = 0;
    ayBuffer[i] = 0;  // Тут инит буферов с 0, чтоб не было ранд. знач
  }
}

void loop() {  // Заloopа
  int16_t ax, ay, az;  // Переменные для данных акселерометра
  mpu.getAcceleration(&ax, &ay, &az);  // Получение данных из MPU6050

  float normAx = (float)ax / 16384.0;
  float normAy = (float)ay / 16384.0;  // Формула для нормализации ускорения, подробно поясню в отдельном файле


  axBuffer[bufferIndex] = normAx;
  ayBuffer[bufferIndex] = normAy;  // Тут запись вышеуказанных норм. x и y d в буфер
  bufferIndex = (bufferIndex + 1) % numReadings;  // И обновление индекса после достижения конца буфера 

  float avgAx = average(axBuffer);
  float avgAy = average(ayBuffer);  // Усреднение значений для уменьшения шума и лучшей точности

  int angle = calculateAngle(avgAx, avgAy);  // Тут на основе уср. знач. рассчитывается угол за счёт функции

  Serial.println(angle);  // Блять, ну тут я пояснять не буду, мы все проггеры

  delay(200);  // Задежка не у девушки - уже хорошо
}

float average(float *buffer) {
  float sum = 0;
  for (int i = 0; i < numReadings; i++) {
    sum += buffer[i];  // Тут происходит cуммирование всех знач. буфера
  }
  return sum / numReadings; // Бля, очень сложная конструкция, хз вообще :(
}  // Это всё нужно для расчёта ср. знач. БУФЕРА

int calculateAngle(float ax, float ay) {  // Угол узнаётся на основе знач. ускорения
  if (ax > 0.5) {  // по оси Х вправо
    return 270;
  } else if (ax < -0.5) {  // по оси Х влево
    return 90;
  } else if (ay > 0.5) {  // по оси Y вправо
    return 0;
  } else {  // по оси Y влево
    return 180;
  }
} 
