import json
import matplotlib.pyplot as plt
from collections import defaultdict

def plot_needs_variance(json_file_path):
    # JSON 파일 읽기
    with open(json_file_path, 'r', encoding='utf-8') as f:
        data = json.load(f)

    # 욕구(Need)로 간주할 스탯 키 목록
    # 게임의 기획에 맞게 필요한 스탯만 남기거나 추가해 주세요.
    need_keys = [
        'hungry', 'fatigue', 'toilet', 'hygiene', 
        'loneliness', 'rLoneliness', 'fun', 'motivation'
    ]
    
    # 캐릭터별 데이터를 저장할 딕셔너리: charName -> { tick: avg_value }
    char_data = defaultdict(lambda: defaultdict(float))
    
    # JSON 데이터 순회하며 계산
    for entry in data.get('chDatas', []):
        tick = entry['tick']
        char_name = entry['charName']
        stats = entry['stats']
        
        sum_sq_diff = 0
        count = 0
        
        # 정의된 need 스탯들에 대해 (100 - need)^2 계산
        for key in need_keys:
            if key in stats:
                val = stats[key]
                sum_sq_diff += (100 - val) ** 2
                count += 1
                
        # 평균 계산
        if count > 0:
            avg_sq_diff = sum_sq_diff / count
            char_data[char_name][tick] = avg_sq_diff

    # 그래프 그리기
    plt.figure(figsize=(10, 6))
    
    for char_name, ticks_data in char_data.items():
        # tick(프레임) 기준으로 정렬
        sorted_ticks = sorted(ticks_data.keys())
        values = [ticks_data[t] for t in sorted_ticks]
        
        # 선 그래프 및 마커 추가
        plt.plot(sorted_ticks, values, label=char_name, marker='o')

    # 그래프 꾸미기
    plt.title('Average of (100 - Need)^2 Over Time')
    plt.xlabel('Tick (Frame)')
    plt.ylabel('Avg( (100 - Need)^2 )')
    plt.legend()
    plt.grid(True, linestyle='--', alpha=0.7)
    plt.tight_layout()
    
    # 그래프 출력 (필요시 plt.savefig("output.png")로 이미지 저장 가능)
    plt.show()

# 실행 예시 (실제 저장된 json 파일 경로를 입력하세요)
if __name__ == "__main__":
    plot_needs_variance('Assets/Log/Log.json')
    pass
