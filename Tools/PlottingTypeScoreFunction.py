import numpy as np
import plotly.graph_objects as go
import plotly.io as pio
from common import *
import os
import json

company_name = "DefaultCompany"
product_name = "School Sim"
log_filename = "log.json"

if os.name == 'nt': 
    base_path = os.path.join(os.environ['USERPROFILE'], 'AppData', 'LocalLow')
else: 
    print("Sorry. Windows Support Only.")
    exit(0)
    
file_path = os.path.join(base_path, company_name, product_name, "Log", log_filename)

print(f"Load file: {file_path}")

    
pio.renderers.default = "browser"

def plot_need_urgency_functions():
    vals = np.linspace(0, 100, 200)

    e_vals = [calc_e_need_multiplier(v) for v in vals]
    r_vals = [calc_r_need_multiplier(v) for v in vals]
    g_vals = [calc_g_need_multiplier(v) for v in vals]

    fig = go.Figure()

    fig.add_trace(go.Scatter(
        x=vals, y=e_vals,
        mode='lines',
        name="E-Need",
        line=dict(width=3, color='crimson')
    ))

    fig.add_trace(go.Scatter(
        x=vals, y=g_vals,
        mode='lines',
        name="G-Need",
        line=dict(width=3, color='mediumseagreen')
    ))

    fig.add_trace(go.Scatter(
        x=vals, y=r_vals,
        mode='lines',
        name="R-Need",
        line=dict(width=3, color='royalblue')
    ))

    fig.update_layout(
        title="Need Urgency Functions",
        xaxis_title="Need Stat Value (0 = Dying, 100 = Full)",
        yaxis_title="Urgency Score",
        legend_title="Need Types",
        font=dict(size=13),
        template="plotly_white",
        hovermode="x unified"
    )

    fig.show()

if __name__ == "__main__":
    plot_need_urgency_functions()