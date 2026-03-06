import numpy as np
import matplotlib.pyplot as plt
from matplotlib.widgets import Slider

n_samples = 15000
r_mu = 1.0     
r_sigma = 0.1   
r_samples = np.random.normal(loc=r_mu, scale=r_sigma, size=n_samples)

b_init = 1.2 
e_init = 0.6 
theta_init_deg = 45

fig, ax = plt.subplots(figsize=(8, 9))
plt.subplots_adjust(bottom=0.35) 

def update_data(b, e, theta_deg):
    theta = np.radians(theta_deg)
    a = b * np.sqrt(1 - e**2) 
    
    x = r_samples * np.cos(theta)
    y = r_samples * np.sin(theta)

    inside = (x**2 / a**2) + (y**2 / b**2) <= 1
    prob = np.mean(inside)
    
    return x, y, a, inside, prob

x_init, y_init, a_init, inside_init, prob_init = update_data(b_init, e_init, theta_init_deg)

t_path = np.linspace(0, 2*np.pi, 200)
ellipse_line, = ax.plot(a_init * np.cos(t_path), b_init * np.sin(t_path), color='black', lw=2)

scat_in = ax.scatter(x_init[inside_init], y_init[inside_init], s=5, color='blue', alpha=0.5, label='Inside')
scat_out = ax.scatter(x_init[~inside_init], y_init[~inside_init], s=5, color='red', alpha=0.5, label='Outside')

guide_line, = ax.plot([0, 2*np.cos(np.radians(theta_init_deg))], 
                      [0, 2*np.sin(np.radians(theta_init_deg))], 
                      color='green', lw=1, ls='--', alpha=0.4)

ax.set_title(f'Prob: {prob_init*100:.2f}%')
ax.set_xlim(-2, 2); ax.set_ylim(-2, 2)
ax.set_aspect('equal')
ax.grid(True)
ax.legend(loc='upper right')

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
    
    new_x, new_y, a, new_inside, new_prob = update_data(b, e, theta_deg)
    
    ellipse_line.set_data(a * np.cos(t_path), b * np.sin(t_path))
    
    if np.any(new_inside):
        scat_in.set_offsets(np.column_stack((new_x[new_inside], new_y[new_inside])))
    else:
        scat_in.set_offsets(np.empty((0, 2))) 
        
    if np.any(~new_inside):
        scat_out.set_offsets(np.column_stack((new_x[~new_inside], new_y[~new_inside])))
    else:
        scat_out.set_offsets(np.empty((0, 2)))
    
    rad = np.radians(theta_deg)
    guide_line.set_data([0, 2*np.cos(rad)], [0, 2*np.sin(rad)])
    ax.set_title(f'Prob: {new_prob*100:.2f}% | a={a:.2f}, b={b:.2f}, e={e:.2f}')
    
    fig.canvas.draw_idle()

    
s_b.on_changed(update)
s_e.on_changed(update)
s_theta.on_changed(update)

plt.show()