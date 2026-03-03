import json
import numpy as np
import plotly.graph_objects as go
import plotly.io as pio
from plotly.subplots import make_subplots
from collections import defaultdict

# 웹 브라우저 렌더러 설정
pio.renderers.default = "browser"

def analyze_strike_zone_impact(json_file_path):
    with open(json_file_path, 'r', encoding='utf-8') as f:
        data = json.load(f)

    # 마지막 프레임(Tick)의 데이터만 사용하여 최종 관계망 분석
    if not data.get('chDatas'):
        print("데이터가 없습니다.")
        return

    last_tick = max(entry['tick'] for entry in data['chDatas'])
    final_data = [e for e in data['chDatas'] if e['tick'] == last_tick]

    char_stats = {}
    rel_matrix = defaultdict(dict) # rel_matrix[A][B] = A가 B를 생각하는 호감도

    # 1. 데이터 파싱
    for entry in final_data:
        name = entry['charName']
        char_id = entry['id']
        char_stats[char_id] = {
            'name': name,
            'attraction': entry.get('attraction', 0.0), # 타원의 장축 (절대적 매력)
            'e': entry.get('e', 0.0),                   # 이심률 (까다로움)
        }
        
        for rel in entry.get('relations', []):
            target_id = rel['rel']['ID']
            # Friend 관계(일반 친밀도)만 분석 (relType == 0 이 Friend라고 가정)
            if rel['rel']['relType'] == 0: 
                rel_matrix[char_id][target_id] = rel['val']

    # 2. 분석용 데이터 배열 구축
    names, attractions, e_values = [], [], []
    incoming_rels = [] # 수신 호감도 (남들이 나를 얼마나 좋아하는가 = 인기)
    outgoing_rels = [] # 발신 호감도 (내가 남들을 얼마나 좋아하는가 = 수용력)
    
    for c_id, stats in char_stats.items():
        names.append(stats['name'])
        attractions.append(stats['attraction'])
        e_values.append(stats['e'])
        
        # Incoming (남 -> 나)
        incoming = [rel_matrix[o_id].get(c_id, 0) for o_id in char_stats if o_id != c_id]
        incoming_rels.append(np.mean(incoming) if incoming else 0)
        
        # Outgoing (나 -> 남)
        outgoing = [rel_matrix[c_id].get(o_id, 0) for o_id in char_stats if o_id != c_id]
        outgoing_rels.append(np.mean(outgoing) if outgoing else 0)

    # 3. 통계 및 상관계수(Pearson) 계산
    corr_attr_incoming = np.corrcoef(attractions, incoming_rels)[0, 1] if len(attractions) > 1 else 0
    corr_e_outgoing = np.corrcoef(e_values, outgoing_rels)[0, 1] if len(e_values) > 1 else 0

    print("=== ⚾ 스트라이크 존(Strike Zone) 시스템 검증 ===")
    print(f"1. [객관적 매력] 매력도(Attraction) vs 수신 호감도(인기) 상관계수: {corr_attr_incoming:.2f}")
    print(f"   -> 양수(+)여야 정상! (타원의 장축이 길수록 타인의 스트라이크 존에 들어가기 쉬움)")
    
    print(f"2. [주관적 취향] 까다로움(e) vs 발신 호감도(수용력) 상관계수: {corr_e_outgoing:.2f}")
    print(f"   -> 음수(-)여야 정상! (e가 1에 가까울수록 타원 폭이 바늘처럼 좁아져 남을 쉽게 안 좋아함)")

    # 짝사랑(비대칭성) 수치 계산
    asymmetry_scores = []
    char_ids = list(char_stats.keys())
    for i in range(len(char_ids)):
        for j in range(i + 1, len(char_ids)):
            id1, id2 = char_ids[i], char_ids[j]
            a_to_b = rel_matrix[id1].get(id2, 0)
            b_to_a = rel_matrix[id2].get(id1, 0)
            asymmetry_scores.append(abs(a_to_b - b_to_a))
    
    print(f"3. [비대칭성] 평균 짝사랑 격차(관계도 차이): {np.mean(asymmetry_scores):.2f}")
    print(f"   -> 0보다 확연히 커야 정상! (각자의 이상형 각도와 e 값이 다르기 때문)")

    # 4. 시각화 (Plotly)
    fig = make_subplots(
        rows=1, cols=2, 
        subplot_titles=(
            f"Attraction vs Popularity (Corr: {corr_attr_incoming:.2f})", 
            f"Eccentricity (e) vs Pickiness (Corr: {corr_e_outgoing:.2f})"
        )
    )

    # 그래프 1: 매력(Attraction) vs 인기(Incoming)
    fig.add_trace(go.Scatter(
        x=attractions, y=incoming_rels,
        mode='markers+text',
        name='Characters',
        text=names,
        textposition='top right',
        marker=dict(size=12, color='royalblue', opacity=0.7),
        hovertemplate="<b>%{text}</b><br>Attraction: %{x:.2f}<br>Popularity: %{y:.2f}<extra></extra>"
    ), row=1, col=1)

    # 그래프 1 추세선
    if len(attractions) > 1:
        z1 = np.polyfit(attractions, incoming_rels, 1)
        p1 = np.poly1d(z1)
        x_range1 = np.linspace(min(attractions), max(attractions), 100)
        fig.add_trace(go.Scatter(
            x=x_range1, y=p1(x_range1),
            mode='lines',
            line=dict(color='red', dash='dash', width=2),
            name='Trendline (Attraction)',
            hoverinfo='skip'
        ), row=1, col=1)

    # 그래프 2: 까다로움(e) vs 수용력(Outgoing)
    fig.add_trace(go.Scatter(
        x=e_values, y=outgoing_rels,
        mode='markers+text',
        name='Characters',
        text=names,
        textposition='top right',
        marker=dict(size=12, color='crimson', opacity=0.7),
        hovertemplate="<b>%{text}</b><br>Eccentricity (e): %{x:.2f}<br>Pickiness (Outgoing): %{y:.2f}<extra></extra>"
    ), row=1, col=2)

    # 그래프 2 추세선
    if len(e_values) > 1:
        z2 = np.polyfit(e_values, outgoing_rels, 1)
        p2 = np.poly1d(z2)
        x_range2 = np.linspace(min(e_values), max(e_values), 100)
        fig.add_trace(go.Scatter(
            x=x_range2, y=p2(x_range2),
            mode='lines',
            line=dict(color='blue', dash='dash', width=2),
            name='Trendline (Eccentricity)',
            hoverinfo='skip'
        ), row=1, col=2)

    # 전체 레이아웃 설정
    fig.update_layout(
        title_text="⚾ Strike Zone System Impact Analysis",
        template="plotly_white",
        showlegend=False,
        height=600,
        hovermode="closest"
    )

    # 축 설정
    fig.update_xaxes(title_text="Attraction (Ellipsis b-axis)", row=1, col=1)
    fig.update_yaxes(title_text="Avg Incoming Relation (Popularity)", row=1, col=1)
    
    fig.update_xaxes(title_text="Eccentricity 'e' (Closer to 1 = Picky)", row=1, col=2)
    fig.update_yaxes(title_text="Avg Outgoing Relation (How much they like others)", row=1, col=2)

    fig.show()

if __name__ == "__main__":
    # 로그 파일 경로를 입력하여 실행하세요.
    analyze_strike_zone_impact('../Assets/Log/Log.json')
    pass