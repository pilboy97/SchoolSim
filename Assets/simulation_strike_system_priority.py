import numpy as np
import matplotlib.pyplot as plt
from matplotlib.widgets import Slider

# --- 1. 초기 설정값 ---
init_e = 0.7  # 초기 이심률 (0 ~ 1 사이)
init_b = 1.2  # 초기 장축 반지름

# --- 2. 확률 계산 함수 (이심률 e 사용) ---
def calculate_prob(e, b):
    # e는 0 이상 1 미만이어야 함
    if e >= 1.0:
        return 0.0, 0.0, "Invalid (e >= 1)"
    
    # 이심률 공식을 이용해 단축 반지름 a 계산
    # a = b * sqrt(1 - e^2)
    a = b * np.sqrt(1 - e**2)
    
    # 초점 거리 f (계산용)
    f = b * e

    # Case 1: 타원이 원을 완전히 덮음 (a >= 1)
    if a >= 1:
        return 1.0, a, "100% (Fully Cover)"
    
    # Case 2: 타원이 원보다 완전히 작음 (b <= 1)
    if b <= 1:
        return 0.0, a, "0% (Too Small)"
        
    # Case 3: 타원이 원에 걸쳐 있음 (a < 1 < b)
    # 근사 공식 변형: f 대신 be 대입
    try:
        if f == 0: # e=0인 경우 (원)
            return (1.0 if b > 1 else 0.0), a, "Circle"
            
        # 기존 공식: (a/f) * sqrt(b^2 - 1)
        val = (a / f) * np.sqrt(b**2 - 1)
        val = np.clip(val, -1.0, 1.0) # 오차 보정
        
        theta_c = np.arccos(val)
        prob = 1 - (2 * theta_c / np.pi)
        return prob, a, f"{prob*100:.1f}% (Approximation)"
    except:
        return 0.0, a, "Error"

# --- 3. 그래프 및 UI 설정 ---
fig, ax = plt.subplots(figsize=(10, 8))
plt.subplots_adjust(left=0.1, bottom=0.25) 

# 단위 원 그리기 (파란 점선 - 타겟 분포)
theta = np.linspace(0, 2*np.pi, 200)
x_circle = np.cos(theta)
y_circle = np.sin(theta)
ax.plot(x_circle, y_circle, 'b--', label='Target Distribution (r=1)')

# 초기 타원 그리기 (빨간 실선)
initial_a = init_b * np.sqrt(1 - init_e**2)
x_ellipse = initial_a * np.cos(theta)
y_ellipse = init_b * np.sin(theta)
line_ellipse, = ax.plot(x_ellipse, y_ellipse, 'r-', lw=2, label='Ellipse')

# 그래프 꾸미기
ax.set_aspect('equal')
ax.set_xlim(-2.5, 2.5)
ax.set_ylim(-2.5, 2.5)
ax.grid(True, alpha=0.3)
ax.legend(loc='upper right')
title_text = ax.set_title(f"Probability simulation", fontsize=14)

# 텍스트 정보 표시
info_template = "Eccentricity (e): {e:.2f}\nSemi-major (b): {b:.2f}\nSemi-minor (a): {a:.2f}"
text_info = ax.text(0.05, 0.95, "", transform=ax.transAxes, verticalalignment='top', 
                    bbox=dict(boxstyle='round', facecolor='wheat', alpha=0.5))

# --- 4. 슬라이더 만들기 ---
ax_e = plt.axes([0.2, 0.1, 0.65, 0.03], facecolor='lightgoldenrodyellow')
ax_b = plt.axes([0.2, 0.05, 0.65, 0.03], facecolor='lightgoldenrodyellow')

# e는 0(원) ~ 0.99(매우 납작)
s_e = Slider(ax_e, 'Eccentricity (e)', 0.0, 0.99, valinit=init_e, valstep=0.01)
s_b = Slider(ax_b, 'Semi-major (b)', 0.5, 3.0, valinit=init_b, valstep=0.01)

# --- 5. 업데이트 함수 ---
def update(val):
    e = s_e.val
    b = s_b.val
    
    # 1. 확률 및 a 계산
    prob, a, status_str = calculate_prob(e, b)
    
    # 2. 타원 다시 그리기
    x_new = a * np.cos(theta)
    y_new = b * np.sin(theta)
    line_ellipse.set_xdata(x_new)
    line_ellipse.set_ydata(y_new)

    # 3. 텍스트 업데이트
    title_text.set_text(f"Probability P ≈ {status_str}")
    
    if prob > 0.99: title_text.set_color('green')
    elif prob < 0.01: title_text.set_color('gray')
    else: title_text.set_color('blue')

    text_info.set_text(info_template.format(e=e, b=b, a=a))
    
    fig.canvas.draw_idle()

s_e.on_changed(update)
s_b.on_changed(update)

update(None)
plt.show()
