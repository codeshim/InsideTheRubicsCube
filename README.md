# InsideTheRubicsCube

## 기술 스택 
- Unity


## 외주 진행 기간   
2020.11.08 - 2020.11.30


## 프로젝트 링크   
[영상링크](https://player.vimeo.com/video/487940565?autoplay=1&amp;loop=0&amp;rel=0)  
[웹전시링크](https://sbart-n.com/Exhibition2/index.html)  

  
## 담당 태스크 소개  
- **뒤틀리는 공간 속에서 이동하는 플레이어 조작 스크립팅**  
루빅스큐브 안의 공간을 플레이어가 돌아다닐 수 있게 만든 컨텐츠로 큐브들이 회전되고 재정렬되어도 중력이 플레이어의 아래 방향으로 작동해야하는 과제였다. 문제를 해결하기 위하여 플레이어의 Rigid Body의 Gravity 항목을 비활성화하고 구충돌과 Raycast를 통해 분별한 현재 올라서 있는 땅이나 사다리 오브젝트를 Parent로 설정하여 Parent 오브젝트의 회전값이나 위치값에 영향을 받지 않도록 했다. 그리고 플레이어가 땅에 있을 때 사다리와 일정거리 가까워지면 사다리에 올라타 수직이동을 할 것인지 묻고, 플레이어가 사다리에 있을 때 땅 위로 올라설 수 있는 거리에서 사다리에서 내려 수평이동을 할 것인지 묻는 것도 함께 구현했다.
<div>
<img width="500" alt="rubics1" src="https://user-images.githubusercontent.com/76104907/102384032-97c6b000-400f-11eb-9e5c-cf2932b9cc7e.png">
<img width="500" alt="rubics2" src="https://user-images.githubusercontent.com/76104907/102384207-cba1d580-400f-11eb-83f5-d7a88e011e1f.png">
<img width="500" alt="rubics3" src="https://user-images.githubusercontent.com/76104907/102386800-ede92280-4012-11eb-9dd3-36d070b5999d.png">
<img width="500" alt="rubics4" src="https://user-images.githubusercontent.com/76104907/102387078-4a4c4200-4013-11eb-8703-986bc13bde8f.png">
</div>
