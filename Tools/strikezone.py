import numpy as np
import matplotlib.pyplot as plt
from matplotlib.widgets import Slider

# 1. 점 데이터 준비 (선분 위의 r 분포)
n_samples = 15000
r_mu = 1.0          # r의 평균
r_sigma = 0.1       # r의 표준편차
# r 값은 고정된 난수로 생성하여 슬라이더 조절 시 점들의 상대적 분포가 유지되게 함
r_samples = np.random.normal(loc=r_mu, scale=r_sigma, size=n_samples)

# 2. 초기 파라미터 설정
b_init = 1.2        # 장반경 (y축)
e_init = 0.6        # 이심률
theta_init_deg = 45 # 각도 (degree)

# 3. 그래프 초기 설정
fig, ax = plt.subplots(figsize=(8, 9))
plt.subplots_adjust(bottom=0.35) # 슬라이더 3개를 위한 여백

# 타원 및 점 데이터를 계산하는 함수
def update_data(b, e, theta_deg):
    theta = np.radians(theta_deg)
    a = b * np.sqrt(1 - e**2)  # 단반경 (x축)
    
    # 좌표 계산 (특정 각도의 선분 위)
    x = r_samples * np.cos(theta)
    y = r_samples * np.sin(theta)
    
    # 타원 내부 판별 (장축이 y축인 경우)
    inside = (x**2 / a**2) + (y**2 / b**2) <= 1
    prob = np.mean(inside)
    
    return x, y, a, inside, prob

# 초기 계산
x_init, y_init, a_init, inside_init, prob_init = update_data(b_init, e_init, theta_init_deg)

# 타원 경계선 그리기
t_path = np.linspace(0, 2*np.pi, 200)
ellipse_line, = ax.plot(a_init * np.cos(t_path), b_init * np.sin(t_path), color='black', lw=2)

# 산점도 (선분 위)
scat_in = ax.scatter(x_init[inside_init], y_init[inside_init], s=5, color='blue', alpha=0.5, label='Inside')
scat_out = ax.scatter(x_init[~inside_init], y_init[~inside_init], s=5, color='red', alpha=0.5, label='Outside')

# 가이드 라인 (각도 방향 선분)
guide_line, = ax.plot([0, 2*np.cos(np.radians(theta_init_deg))], 
                      [0, 2*np.sin(np.radians(theta_init_deg))], 
                      color='green', lw=1, ls='--', alpha=0.4)

ax.set_title(f'Prob: {prob_init*100:.2f}%')
ax.set_xlim(-2, 2); ax.set_ylim(-2, 2)
ax.set_aspect('equal')
ax.grid(True)
ax.legend(loc='upper right')

# 4. 슬라이더 추가
# 슬라이더 위치 설정 [left, bottom, width, height]
ax_b = plt.axes([0.2, 0.22, 0.6, 0.03])
ax_e = plt.axes([0.2, 0.16, 0.6, 0.03])
ax_theta = plt.axes([0.2, 0.1, 0.6, 0.03])

s_b = Slider(ax_b, 'Major axis (b)', 0.5, 1.8, valinit=b_init)
s_e = Slider(ax_e, 'Eccentricity (e)', 0.0, 0.99, valinit=e_init)
s_theta = Slider(ax_theta, 'Angle (θ)', 0, 360, valinit=theta_init_deg)
def update(val):
    b = s_b.val
    e = s_e.val
    theta_deg = s_theta.val
    
    # 1. 데이터 재계산 (현재 슬라이더 값 반영)
    new_x, new_y, a, new_inside, new_prob = update_data(b, e, theta_deg)
    
    # 2. 타원 모양 업데이트
    ellipse_line.set_data(a * np.cos(t_path), b * np.sin(t_path))
    
    # 3. 산점도 위치 업데이트 (중요!)
    # Inside 점들은 Inside의 x와 y만 묶어야 함
    if np.any(new_inside): # Inside 점이 하나라도 있을 때
        scat_in.set_offsets(np.column_stack((new_x[new_inside], new_y[new_inside])))
    else:
        scat_in.set_offsets(np.empty((0, 2))) # 없으면 빈 배열
        
    # Outside 점들은 Outside의 x와 y만 묶어야 함
    if np.any(~new_inside): # Outside 점이 하나라도 있을 때
        scat_out.set_offsets(np.column_stack((new_x[~new_inside], new_y[~new_inside])))
    else:
        scat_out.set_offsets(np.empty((0, 2)))
    
    # 4. 가이드 라인 및 타이틀 업데이트
    rad = np.radians(theta_deg)
    guide_line.set_data([0, 2*np.cos(rad)], [0, 2*np.sin(rad)])
    ax.set_title(f'Prob: {new_prob*100:.2f}% | a={a:.2f}, b={b:.2f}, e={e:.2f}')
    
    fig.canvas.draw_idle()

    
s_b.on_changed(update)
s_e.on_changed(update)
s_theta.on_changed(update)

plt.show()