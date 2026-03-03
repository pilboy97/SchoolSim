import json
import matplotlib.pyplot as plt
from collections import defaultdict

# ==========================================
# C# 로직과 동일한 위급도(Multiplier) 계산 함수
# ==========================================
def calc_e_need_multiplier(val):
    """
    생존 욕구 (Hungry, Fatigue, Hygiene, Toilet)
    64를 기준으로 지수적으로 급증함. 
    값이 낮아질수록(0에 가까워질수록) 위급도가 폭발적으로 커짐.
    """
    return 0.3 ** ((val - 64.0) / 3.0)

def calc_r_need_multiplier(val):
    """
    관계 욕구 (Fun, Loneliness, rLoneliness)
    100에서 멀어질수록 선형적으로 증가함. 최대치 200.
    """
    return max(0.0, 2.0 * (100.0 - val))

# 파이썬 스크립트의 이 부분을 수정하세요!
def calc_g_need_multiplier(val):
    ratio = (100.0 - val) / 100.0
    return 10.0 + ((ratio ** 3) * 2500.0)

# ==========================================
# 데이터 파싱 및 그래프 그리기
# ==========================================
def plot_total_disutility(json_file_path):
    with open(json_file_path, 'r', encoding='utf-8') as f:
        data = json.load(f)

    # 욕구 분류 (C# 코드의 구조 기준)
    e_needs = ['hungry', 'fatigue', 'toilet', 'hygiene']
    r_needs = ['fun', 'loneliness', 'rLoneliness']
    g_needs = ['motivation']
    
    # 캐릭터별 데이터를 저장할 딕셔너리: charName -> { tick: total_disutility }
    char_data = defaultdict(lambda: defaultdict(float))
    
    for entry in data.get('chDatas', []):
        tick = entry['tick']
        char_name = entry['charName']
        stats = entry.get('stats', {})
        
        total_disutility = 0.0
        
        # 1. 생존 욕구 위급도 합산
        for key in e_needs:
            if key in stats:
                total_disutility += calc_e_need_multiplier(stats[key])
                
        # 2. 관계 욕구 위급도 합산
        for key in r_needs:
            if key in stats:
                total_disutility += calc_r_need_multiplier(stats[key])
                
        # 3. 성장/동기 욕구 위급도 합산
        for key in g_needs:
            if key in stats:
                total_disutility += calc_g_need_multiplier(stats[key])
                
        # 해당 프레임(Tick)에서의 캐릭터 총 위급도 기록
        char_data[char_name][tick] = total_disutility

    # 그래프 생성
    plt.figure(figsize=(12, 7))
    
    for char_name, ticks_data in char_data.items():
        sorted_ticks = sorted(ticks_data.keys())
        values = [ticks_data[t] for t in sorted_ticks]
        
        plt.plot(sorted_ticks, values, label=char_name, marker='o', markersize=4)

    # 그래프 꾸미기
    plt.title('Total Disutility (Need Urgency Multipliers) Over Time')
    plt.xlabel('Tick (Frame)')
    plt.ylabel('Total Urgency Score (Lower is Better)')
    plt.legend()
    plt.grid(True, linestyle='--', alpha=0.7)
    plt.tight_layout()
    
    # 출력
    plt.show()

if __name__ == "__main__":
    # 실제 로그 파일의 경로를 입력해 주세요.
    plot_total_disutility('Assets/Log/Log.json')
    pass
