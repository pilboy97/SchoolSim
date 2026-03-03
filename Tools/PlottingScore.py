import numpy as np
import plotly.graph_objects as go
import plotly.io as pio
from common import * # calc_e_need_multiplier 등 욕구 점수 함수 임포트

# 웹 브라우저 렌더러 설정
pio.renderers.default = "browser"

def plot_need_urgency_functions():
    # ==========================================
    # 1. 스탯 값 범위 설정 (0 ~ 100 구간을 200개로 쪼갬)
    # ==========================================
    vals = np.linspace(0, 100, 200)

    # 각 스탯 값에 따른 욕구별 위급도 점수 계산
    e_vals = [calc_e_need_multiplier(v) for v in vals]
    r_vals = [calc_r_need_multiplier(v) for v in vals]
    g_vals = [calc_g_need_multiplier(v) for v in vals]

    # ==========================================
    # 2. Plotly Figure 생성 및 데이터 추가
    # ==========================================
    fig = go.Figure()

    # E-Need (생존 욕구) 곡선 추가
    fig.add_trace(go.Scatter(
        x=vals, y=e_vals,
        mode='lines',
        name="E-Need (Survival - 5000 Max)",
        line=dict(width=3, color='crimson')
    ))

    # G-Need (동기 욕구) 곡선 추가
    fig.add_trace(go.Scatter(
        x=vals, y=g_vals,
        mode='lines',
        name="G-Need (Motivation - 2500 Max)",
        line=dict(width=3, color='mediumseagreen')
    ))

    # R-Need (관계 욕구) 선 추가
    fig.add_trace(go.Scatter(
        x=vals, y=r_vals,
        mode='lines',
        name="R-Need (Relation - 1000 Max)",
        line=dict(width=3, color='royalblue')
    ))

    # ==========================================
    # 3. 레이아웃(UI) 설정
    # ==========================================
    fig.update_layout(
        title="Need Urgency Functions (Log Scale)",
        xaxis_title="Need Stat Value (0 = Dying, 100 = Full)",
        yaxis_title="Urgency Score",
        yaxis_type="log", # Y축 로그 스케일 적용 (값 차이가 커도 한눈에 비교 가능)
        legend_title="Need Types",
        font=dict(size=13),
        template="plotly_white", # 깔끔한 바탕
        hovermode="x unified" # 특정 스탯 값에 마우스를 올리면 세 가지 욕구 점수를 동시에 비교
    )

    # 4. 브라우저에 그래프 출력
    fig.show()

if __name__ == "__main__":
    plot_need_urgency_functions()