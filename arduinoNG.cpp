#include <Wire.h>
#include <MPU6050.h>

MPU6050 mpu;

// Константы
const float smoothingFactor = 0.1; // Коэффициент сглаживания
float avgAx = 0, avgAy = 0;        // Для скользящего среднего
int lastAngle = -1;                // Последний стабильный угол

void setup() {
  Serial.begin(9600);
  Wire.begin();
  mpu.initialize();

  if (!mpu.testConnection()) {
    Serial.println("Ошибка подключения MPU6050.");
    while (1);
  }

  Serial.println("MPU6050 подключен.");
}

void loop() {
  int16_t ax, ay, az;
  mpu.getAcceleration(&ax, &ay, &az);

  // Нормализация значений
  float normAx = ax / 16384.0;
  float normAy = ay / 16384.0;

  // Скользящее среднее
  avgAx += smoothingFactor * (normAx - avgAx);
  avgAy += smoothingFactor * (normAy - avgAy);

  // Вычисление текущего угла
  int currentAngle = calculateAngle(avgAx, avgAy);

  // Фильтрация изменений угла
  if (currentAngle != lastAngle) {
    static int stabilityCounter = 0; // Счётчик для проверки стабильности
    stabilityCounter++; // Увеличиваем счётчик при изменении угла

    if (stabilityCounter > 3) { // Угол должен оставаться неизменным несколько циклов
      lastAngle = currentAngle; // Подтверждаем новое значение угла
      stabilityCounter = 0; // Сбрасываем счётчик
      Serial.print("Угол: ");
      Serial.println(lastAngle);
    }
  } else {
    // Если угол остался прежним, сбрасываем счётчик
    static int stabilityCounter = 0;
  }

  delay(50); // Задержка между циклами
}

// Функция вычисления угла
int calculateAngle(float ax, float ay) {
  if (ax > 0.5) {
    return 270;
  } else if (ax < -0.5) {
    return 90;
  } else if (ay > 0.5) {
    return 0;
  } else {
    return 180;
  }
}
