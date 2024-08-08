<html>

	<head>
		<title>PhPStudy_2024_08_08</title>
	</head>
	
	
	<body>
		<h2>문법 연습</h2>

		<?
			//1. 출력하기
			echo "Hellooo,반가워요.<br>";
			print "print 작동<br>";

			//2. 변수 선언
			//변수 사용하기 위해 변수타입지정 필요하지않음.+변수를 사용하기 위해 그 변수명 앞에 $표기를 붙여야함.
			echo"<br><br>";
			
			$name = "와이번";
			$data1 = 1 + 2;
			$data2 = $data1 / 4;
			
			echo $name. "<br>";
			echo $data1 . "<br>";
			echo "$data2<br>";

			$name = 123 +2;
			echo "$name<br>";

			//3. 문자열 연결 연산자
			echo "<br><br>";
			
			$data1 = "천사의";
			$data2 = $data1 . "반지";

			echo $data2;

			$name = "다니엘";
			echo "<br>이름은 ". $name. "입니다.";
			
			//""와 '' 사용
			echo "<br><br>";
			$AAA ="아기 상어";
			$BBB = '아론 상어';
			echo $AAA . " : " .$BBB;

			//##차이점
			$a = 111;
			echo "<br>$a"; //내용 출력
			$a = 321;
			echo '<br>$a'; //출력시 변수 이름자체가 출력됨.

			//4. 변수 형변환
			echo"<br><br>";
			$data1 = 3.14;
			echo $data1. " <br>";
			$data2 = (int)$data1;
			echo $data2;	

			//5. 연산자
			echo"<br><br>";
			$data1 = 5*7;
			$data2= 32 / (1+2);
			$data3 = $data4 = 3;
			
			echo $data1 . "<br>";
			echo $data2 . "<br>";
			echo $data3 . "<br>";
			
			//6.for문
			echo"<br><br>";
			$sum = 0;
			for($i = 1; $i <= 10; $i++)
			{
				$sss = $i;
				$sum += $i;
			}
			echo "for문 사용 : $sum : $sss";
			
			//7. While문
			echo"<br><br>";
			$k= 0;

			//$x = 0;
			while(1)
			{
				$k++;
				if(10< $k)
				break;
				$x= $k;
			}
			echo "변수 x에 저장한 값은 $x 입니다.";
			
			//8.배열
			echo"<br><br>";
			$AAA = array();
			$AAA[0] = 34;
			$AAA[1] = 58;
			$AAA[2] = 123;

			$Arr[] =42; 
			$Arr[] = 73;
			$Arr[] = 100;
			$Arr[0] = 37;
			$Arr[1] = 25;

			echo $Arr[0] . "<p>";
			echo $Arr[1] . "<p>";
			echo $Arr[2] . "<p>";
			
			$fruit["a"] = "사과";
			$fruit["b"] = "바나나";
			
			echo $fruit["a"] . "<p>";
			echo $fruit["b"] . "<p>";
			
			echo"Arr 배열의 갯수 : " . count( $Arr);

			//9. 배열의 암시적 선언

			echo "<br><br>";
			$hobby = array("영화감상" , "등산", "게임");
			
			//3개의 값을 배열에 등록해서 hobby배열 변수를 만들어주기
			echo $hobby[0] . "<br>";
			echo $hobby[1] . "<br>";
			echo $hobby[2] . "<br>";

			//10. 배열의 참조변수
			echo "<br><br>";
			$brray = array();
			$score = &$brray;
			
			$score[0] = 24;
			$score[1] = 83;
			$score[2] = 92;
			$score[3] = 73;
			$score[4] = 29;
			$score[5] = 72;
			$score[6] = 62;
			$score[7] = 53;

			$sum = 0;
			for($i=0; $i < 10; $i++)
			{
				echo "[$i] : " . $brray[$i] . "<br>";
				$sum = $sum + $brray[$i];
			}
			echo "배열 합 테스트 : " . $sum;
			
			//11. 배열 지정 인덱스
			echo "<br><br>";
			$flower = array("장미","개나리","진달래", 2=> "해바라기" , "튤립");
			echo $flower[0]. "<p>"; //장미
			echo $flower[1]. "<p>"; //개나리
			echo $flower[2]. "<p>"; //진달래인 값이 덮어씌워져 해바라기가 됨.
			echo $flower[3]. "<p>"; //튤립
			echo $flower[4]. "<p>"; //없음 null이 출력


			//12. 함수 : 두변수를 합산해서 출력함수 예제
			function plus($a, $b)
			{
				$c = $a + $b;
				echo $c . "<p>";
			}
			
			plus(5, 19);
			plus (4,34);
			
			//echo $c . "<br>"; //함수내의 지역변수는 접근 불가

			//13. return 받아오기
			function Multiple($a, $b)
			{
				$c = $a * $b;
				return $c;	
			}
			$Rst = Multiple(23, 46);
			echo "Multiple() 결과 : " . $Rst . "<br>";
			
			function DivRest($a , $b)
			{
				$div = intval($a / $b);
				$rest = $a % $b;

				return array($div , $rest);
				
			}
			
			$MArr = DivRest(30 , 7);
			echo "몫 = " . $MArr[0] . "<br>";
			echo "나머지 =" .$MArr[1] . "<br>";
			
			//14. 지역 변수를 전역변수로 선언하는 global
			echo "<br><br>";
			$data1 = "전역변수";
			
			function MyFunc2()
			{
				global $data1; //글로벌 변수르 의미한다를 명시화'
				$data1 = "글로벌 변수가 맞나요?"; //MyFunc2라는 지역변수
				echo "$data1<br>";
			}
			
			MyFunc2();
			echo $data1;	

			// 15. MySQL 사용을 위한 함수들
			//1.die() ; php 스크립트의 실행을 즉시 중지
			//2.mysqli_connect() ; MySQL 데이터베이스 접속
			//3.mysqli_connect_error() ; 서버에 접근 오류를 반환하는 함수
			//4.mysqli_query() ; SQL 명령어 쿼리문을 실행하는 함수	
			//5.mysqli_num_rows() ; 데이터베이스에서 쿼리를 보내고 나온 레코드 개수 (0일시 행의 값이 없다는 뜻)	
			//6.mysqli_fetch_assoc(); MySQL의 실행 결과에서 결과 행을 가져옴
			//7..mysqli_close() ; MySQL 서버 종료

	
		?>
	</body>
</html>