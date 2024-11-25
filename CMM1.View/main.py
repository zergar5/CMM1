import numpy as np
import matplotlib.pyplot as plt
from scipy.interpolate import griddata

# Функция для чтения данных из файла
def read_data(filename):
    x, y, z = [], [], []
    with open(filename, 'r') as file:
        for line in file:
            data = line.strip().split()
            x.append(float(data[0]))
            y.append(float(data[1]))
            z.append(float(data[2]))
    return np.array(x), np.array(y), np.array(z)

# Функция для чтения точек из файла
def read_points(filename):
    x, y = [], []
    with open(filename, 'r') as file:
        for line in file:
            data = line.strip().split()
            x.append(float(data[0]))
            y.append(float(data[1]))
    return np.array(x), np.array(y)

directory = "sin(x) + cos(y)/weights/weight = 5"
# Чтение данных из файлов
filename1 = f'{directory}/dataFEM.txt'
filename2 = f'{directory}/dataSpline.txt'
filename3 = f'{directory}/dataTrue.txt'
x1, y1, z1 = read_data(filename1)
x2, y2, z2 = read_data(filename2)
x3, y3, z3 = read_data(filename3)

# Определение минимальных и максимальных значений для x и y
min_x = min(min(x1), min(x2), min(x3))
max_x = max(max(x1), max(x2), max(x3))
min_y = min(min(y1), min(y2), min(y3))
max_y = max(max(y1), max(y2), max(y3))

# Создание сетки для отображения поверхностей с шагом 1
xi = np.arange(min_x, max_x + 1, 1)
yi = np.arange(min_y, max_y + 1, 1)
xi, yi = np.meshgrid(xi, yi)

# Интерполяция данных на сетку
zi1 = griddata((x1, y1), z1, (xi, yi), method='nearest')
zi2 = griddata((x2, y2), z2, (xi, yi), method='nearest')
zi3 = griddata((x3, y3), z3, (xi, yi), method='nearest')

# Определение минимальных и максимальных значений для z
min_z = min(np.min(zi1), np.min(zi2), np.min(zi3))
max_z = max(np.max(zi1), np.max(zi2), np.max(zi3))

# Создание первого изображения для поверхностей
fig1 = plt.figure(figsize=(8, 6))
ax1 = fig1.add_subplot(111, projection='3d')
surf1 = ax1.plot_surface(xi, yi, zi1, color='blue', label='FEM')
surf2 = ax1.plot_surface(xi, yi, zi2, color='red', label='Spline')
surf3 = ax1.plot_surface(xi, yi, zi3, color='green', label='True')

ax1.set_xlabel('X')
ax1.set_ylabel('Y')
ax1.set_zlabel('Function value')
ax1.set_zlim(min_z, max_z)  # Установка шкалы по оси Z

# Добавление легенды для поверхностей
ax1.legend()

# Создание второго изображения для проекций
fig2 = plt.figure(figsize=(8, 6))
ax2 = fig2.add_subplot(111)

# Проекция по оси X и значению функции
ax2.plot(x1, z1, color='blue', label='FEM')
ax2.plot(x2, z2, color='red', label='Spline')
ax2.plot(x3, z3, color='green', label='True')

ax2.set_xlabel('X')
ax2.set_ylabel('Function value')
ax2.set_ylim(min_z, max_z)  # Установка шкалы по оси Y

# Добавление легенды для проекций
ax2.legend()

ax1.set_xticks(np.arange(min_x, max_x + 1, 1))
ax1.set_yticks(np.arange(min_y, max_y + 1, 1))
ax2.set_xticks(np.arange(min_x, max_x + 1, 1))

# Отображение графиков
plt.tight_layout()
plt.show()