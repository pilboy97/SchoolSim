import json
import numpy as np
import plotly.graph_objects as go

def plot_relationship_network_custom_colors(json_file_path):
    with open(json_file_path, 'r', encoding='utf-8') as f:
        data = json.load(f)

    ch_datas = data.get('chDatas', [])
    if not ch_datas: return

    ticks = sorted(list(set(e['tick'] for e in ch_datas)))
    char_ids = sorted(list(set(e['id'] for e in ch_datas)))
    id_to_name = {e['id']: e['charName'] for e in ch_datas}
    
    # 모든 가능한 관계 쌍 고정 (애니메이션 안정성)
    all_pairs = []
    for i in range(len(char_ids)):
        for j in range(i + 1, len(char_ids)):
            all_pairs.append((char_ids[i], char_ids[j]))

    frames = []
    for tick in ticks:
        tick_entries = {e['id']: e for e in ch_datas if e['tick'] == tick}
        
        # 위치 계산 (원형 배치 + 친밀도 기반 인력/척력)
        pos = {cid: np.array([np.cos(2*np.pi*i/len(char_ids)), 
                              np.sin(2*np.pi*i/len(char_ids))]) * 2.5
               for i, cid in enumerate(char_ids)}

        tick_rel_map = {}
        for cid in char_ids:
            if cid in tick_entries and 'relations' in tick_entries[cid]:
                for r_entry in tick_entries[cid]['relations']:
                    target_id = r_entry['rel']['ID']
                    pair = tuple(sorted((cid, target_id)))
                    tick_rel_map[pair] = r_entry
                    
                    # 물리 효과 보정
                    val = r_entry['val']
                    if cid in pos and target_id in pos:
                        force = val / 200.0
                        pos[cid] = pos[cid] + (pos[target_id] - pos[cid]) * force

        frame_data = []
        for p1, p2 in all_pairs:
            rel = tick_rel_map.get((p1, p2))
            
            if rel and (abs(rel['val']) > 0.1): # 유의미한 관계가 있을 때
                val = rel['val']
                # 로맨스 수치 가져오기 (만약 데이터에 로맨스 수치가 별도로 있다면 사용, 
                # 여기서는 C# relType이 1이면 로맨스 수치가 높다고 가정하거나 별도 필드 확인)
                # 요청하신 규칙: 로맨스 >= 친밀도 * 0.5
                # C# 코드 구조상 Romance 타입인 경우를 로맨스 점수가 높은 것으로 판별
                is_romance_type = (rel['rel']['relType'] == 1)
                
                # 색상 규칙 적용
                if is_romance_type: # 로맨스 관계일 때
                    color = 'rgba(255, 0, 0, 0.8)' # 빨강
                    label = "Romance"
                elif val > 0: # 친밀도 양수
                    color = 'rgba(0, 255, 0, 0.7)' # 초록
                    label = "Friend"
                else: # 친밀도 음수
                    color = 'rgba(0, 0, 0, 0.6)' # 검정
                    label = "Negative"
                
                width = abs(val)/10 + 2
                opacity = 1.0
                hover_text = f"{id_to_name[p1]} - {id_to_name[p2]}<br>Status: {label}<br>Value: {val:.1f}"
            else:
                color = 'rgba(0,0,0,0)'
                width = 0
                opacity = 0
                hover_text = ""

            frame_data.append(go.Scatter(
                x=[pos[p1][0], pos[p2][0], None],
                y=[pos[p1][1], pos[p2][1], None],
                mode='lines',
                line=dict(width=width, color=color),
                hoverinfo='text' if width > 0 else 'skip',
                text=hover_text,
                showlegend=False
            ))

        # 노드 추가
        node_trace = go.Scatter(
            x=[pos[cid][0] for cid in char_ids],
            y=[pos[cid][1] for cid in char_ids],
            mode='markers+text',
            text=[id_to_name[cid] for cid in char_ids],
            textposition="top center",
            marker=dict(size=25, color='white', line=dict(width=2, color='#333')),
            hoverinfo='none'
        )
        frame_data.append(node_trace)
        frames.append(go.Frame(data=frame_data, name=str(tick)))

    # 레이아웃
    fig = go.Figure(
        data=frames[0].data if frames else [],
        layout=go.Layout(
            title="Character Social Network (Red: Romance, Green: Friend, Black: Negative)",
            xaxis=dict(range=[-5, 5], showgrid=False, zeroline=False, showticklabels=False),
            yaxis=dict(range=[-5, 5], showgrid=False, zeroline=False, showticklabels=False),
            hovermode='closest',
            updatemenus=[{
                "type": "buttons",
                "buttons": [
                    {"label": "▶ Play", "method": "animate", "args": [None, {"frame": {"duration": 500, "redraw": True}}]},
                    {"label": "Pause", "method": "animate", "args": [[None], {"frame": {"duration": 0, "redraw": False}}]}
                ],
                "x": 0.05, "y": 0
            }],
            sliders=[{
                "steps": [{"args": [[f.name], {"frame": {"duration": 300, "redraw": True}}],
                           "label": f.name, "method": "animate"} for f in frames],
                "x": 0.15, "y": 0, "len": 0.85
            }]
        ),
        frames=frames
    )

    fig.show()

if __name__ == "__main__":
    plot_relationship_network_custom_colors('../Assets/Log/log.json')