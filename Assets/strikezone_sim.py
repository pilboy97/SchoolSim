import numpy as np
import matplotlib.pyplot as plt
from matplotlib.widgets import Slider
from scipy.stats import norm
from scipy.integrate import quad

# --- 1. 초기 설정값 ---
init_e = 0.7        # 이심률
init_b = 1.2        # 장축 반지름
init_angle = 0.0    # 조준 각도 (도 단위)
std_r = 0.1         # 거리 표준편차
std_theta_deg = 10  # 각도 표준편차 (도)
std_theta_rad = np.deg2rad(std_theta_deg)

# --- 2. 확률 계산 함수 (수치 적분) ---
def calculate_prob(e, b, target_angle_deg):
    if e >= 1.0 or b <= 0: return 0.0, 0.0
    
    a = b * np.sqrt(1 - e**2)
    target_rad = np.deg2rad(target_angle_deg)
    
    # 적분 범위: 평균 주변 ±4표준편차 (99.99% 커버)
    # 전체 360도를 다 할 필요 없이 점이 있는 곳만 적분하면 됨
    lower = target_rad - 4 * std_theta_rad
    upper = target_rad + 4 * std_theta_rad

    def integrand(theta):
        # 1. 해당 각도에서의 타원 한계 반지름 R(theta)
        # 타원 식: r^2 (cos^2/a^2 + sin^2/b^2) = 1
        denom = np.sqrt((np.cos(theta)/a)**2 + (np.sin(theta)/b)**2)
        limit_r = 1.0 / denom
        
        # 2. r이 limit_r보다 작을 확률 (거리 조건)
        prob_r = norm.cdf(limit_r, loc=1.0, scale=std_r)
        
        # 3. 각도가 theta일 확률 밀도 (각도 조건)
        pdf_theta = norm.pdf(theta, loc=target_rad, scale=std_theta_rad)
        
        return prob_r * pdf_theta

    # 적분 수행
    total_prob, _ = quad(integrand, lower, upper)
    return total_prob, a

# --- 3. 그래프 및 UI 설정 ---
fig, ax = plt.subplots(figsize=(10, 8))
plt.subplots_adjust(left=0.1, bottom=0.3) 

# 산점도 (Scatter plot) 초기화
# 시각적 확인을 위해 500개의 랜덤 점 생성
num_points = 500
random_r = np.random.normal(1.0, std_r, num_points)
random_th = np.random.normal(np.deg2rad(init_angle), std_theta_rad, num_points)
x_points = random_r * np.cos(random_th)
y_points = random_r * np.sin(random_th)
scatter = ax.scatter(x_points, y_points, s=10, c='blue', alpha=0.5, label='Points')

# 타원 그리기
theta_line = np.linspace(0, 2*np.pi, 200)
init_a = init_b * np.sqrt(1 - init_e**2)
line_ellipse, = ax.plot(init_a*np.cos(theta_line), init_b*np.sin(theta_line), 'r-', lw=2, label='Ellipse')

# 그래프 꾸미기
ax.set_aspect('equal')
lim = 2.0
ax.set_xlim(-lim, lim)
ax.set_ylim(-lim, lim)
ax.grid(True, alpha=0.3)
ax.legend(loc='upper right')
title_text = ax.set_title("Probability Simulation", fontsize=14)

# --- 4. 슬라이더 ---
ax_e = plt.axes([0.2, 0.15, 0.65, 0.03], facecolor='lightgoldenrodyellow')
ax_b = plt.axes([0.2, 0.10, 0.65, 0.03], facecolor='lightgoldenrodyellow')
ax_ang = plt.axes([0.2, 0.05, 0.65, 0.03], facecolor='lightblue')

s_e = Slider(ax_e, 'Eccentricity (e)', 0.0, 0.99, valinit=init_e)
s_b = Slider(ax_b, 'Semi-major (b)', 0.5, 2.5, valinit=init_b)
s_ang = Slider(ax_ang, 'Target Angle (°)', 0.0, 360.0, valinit=init_angle)

# --- 5. 업데이트 함수 ---
def update(val):
    e = s_e.val
    b = s_b.val
    angle = s_ang.val
    
    # 1. 확률 계산 (수치 적분)
    prob, a = calculate_prob(e, b, angle)
    
    # 2. 타원 업데이트
    x_new = a * np.cos(theta_line)
    y_new = b * np.sin(theta_line)
    line_ellipse.set_xdata(x_new)
    line_ellipse.set_ydata(y_new)
    
    # 3. 점 분포 업데이트 (시각 효과용 다시 뿌리기)
    # 슬라이더 움직일 때마다 점들이 해당 각도로 이동
    new_th = np.random.normal(np.deg2rad(angle), std_theta_rad, num_points)
    new_r = np.random.normal(1.0, std_r, num_points) # r은 재생성 안 해도 되지만 통일성 위해
    
    scatter.set_offsets(np.c_[new_r * np.cos(new_th), new_r * np.sin(new_th)])
    
    # 색상 변경: 타원 내부 점은 빨강, 외부 점은 파랑 (시각적 근사)
    # 점 하나하나 검사
    # (x/a)^2 + (y/b)^2 <= 1
    pts_x = new_r * np.cos(new_th)
    pts_y = new_r * np.sin(new_th)
    is_inside = (pts_x/a)**2 + (pts_y/b)**2 <= 1
    
    # scatter 색상 배열 업데이트 (내부: 빨강, 외부: 파랑)
    colors = np.array(['blue'] * num_points)
    colors[is_inside] = 'red'
    scatter.set_color(colors)
    
    # 4. 텍스트 업데이트
    title_text.set_text(f"Probability: {prob*100:.2f}% (Target: {angle:.0f}°)")
    if prob > 0.9: title_text.set_color('green')
    elif prob < 0.1: title_text.set_color('gray')
    else: title_text.set_color('black')
    
    fig.canvas.draw_idle()

s_e.on_changed(update)
s_b.on_changed(update)
s_ang.on_changed(update)

update(None)
plt.show()
