<?php

	
	$con = mysqli_connect("localhost", "typhoon", "jun3824!", "typhoon");
	if(!$con)
	{
		die("Could not Connect " . mysqli_connect_error());
	}

	
	if(isset($_POST['Item_Input_Msg']))
	{
		echo '<div id="Item_Input_Msg" style="color:green;">' . htmlspecialchars($_POST['Item_Input_Msg']) .'</div><br>';
		echo '<script>
			setTimeout( function()
				    {
					document.getElementById("Item_Input_Msg").style.display="none";
				    }, 
				    5000);
		         </script>';		
		unset($_POST['Item_Input_Msg']);
	}

	if(isset($_POST['update_message']))
	{
		echo '<div id="update_message" style="color:green;">' . htmlspecialchars($_POST['update_message']) .'</div><br>';
		echo '<script>
			setTimeout( function()
				    {
					document.getElementById("update_message").style.display="none";
				    }, 
				    5000);
		         </script>';		
		unset($_POST['update_message']);
	}

	if(isset($_POST['del_message']))
	{
		echo '<div id="del_message" style="color:red;">' . htmlspecialchars($_POST['del_message']) .'</div><br>';
		echo '<script>
			setTimeout( function()
				    {
					document.getElementById("del_message").style.display="none";
				    }, 
				    5000);
		         </script>';		
		unset($_POST['del_message']);
	}

	// 전체를 중앙 정렬하기 위한 컨테이너
	echo '<div style="text-align: center">';	 //중앙 정렬 시작
	// 테이블과 버튼들을 포함하는 컨테이너 정렬
	echo '<div style="display:inline-block; text-align: left;">'; //테이블과 버튼들을 묶음

	echo '<form method="post" action="Item_Input.php">';
	echo '<input type="submit" value="아이템 추가하기">';
	echo '</form>';


	echo '<form name="Item_List" method="post">';

	//100개 리스트 가져오기
	$LowList = mysqli_query($con, "SELECT * FROM ItmeGrades ORDER BY idx DESC LIMIT 0, 100");  //0번부터 100개까지 가져오라는 의미
	
	if(!$LowList)
		echo "쿼리오류 발생 : " . mysqli_error($con);	

	echo  '<table border="1" width="710" height="10" cellspacing="0">'; 
	//강제로라도 height="10" 안주면 이상하게 입력상자의 위아래에 여백이 생겨 버린다.
	echo '<tr align="center" bgcolor="blue">';
		echo '<td style="width: 60px; height: 30px; color: white">선택</td>';
		echo '<td style="width: 80px; color: white">고유번호</td>';
		echo '<td style="width: 170px; color: white">아이템이름</td>';
		echo '<td style="color: white">레벨</td>';
		echo '<td style="color: white">공격상승률</td>';
		echo '<td style="color: white">가격</td>';
	echo '</tr>';

	$rowsCount = mysqli_num_rows($LowList);
	for($a_ii = 0; $a_ii < $rowsCount; $a_ii++)
	{
		$a_row = mysqli_fetch_array($LowList);	//행 정보 하나씩 가져오기
		if($a_row != false)
		{
			$UniqueId = $a_row["idx"];

			echo '<tr align="center">';
				echo '<td style="height: 30px;">';
				echo '<input type="radio" name="DelItName" value="'.$UniqueId.'">';
				echo '</td>';
				echo '<td>';
				echo $a_row['idx'];
				echo '</td>';

				//htmlspecialchars() 함수 : 사용자 입력값을 안전하게 표시하기 위해 사용
				echo '<td style="padding: 0;"><input type="text" maxlength="12" name="ItemName_' . $UniqueId . '" 
				value="' . htmlspecialchars($a_row['ItemName']) . '" 
				style="width: 100%; height: 100%; text-align: center; border: 0px;"></td>'; 

				echo '<td style="padding: 0;"><input type="text" maxlength="12" name="ItemLevel_' . $UniqueId . '" 
				value="' . htmlspecialchars($a_row['ItemLevel']) . '" 
				style="width: 100%; height: 100%; text-align: center; border: 0px;"></td>'; 

				echo '<td style="padding: 0;"><input type="text" maxlength="12" name="ItemAttRate_' . $UniqueId . '" 
				value="' . htmlspecialchars($a_row['ItemAttRate']) . '" 
				style="width: 100%; height: 100%; text-align: center; border: 0px;"></td>'; 

				echo '<td style="padding: 0;"><input type="text" maxlength="12" name="ItemPrice_' . $UniqueId . '" 
				value="' . htmlspecialchars($a_row['ItemPrice']) . '" 
				style="width: 100%; height: 100%; text-align: center; border: 0px;"></td>'; 
				
			echo '</tr>';
			
		}//if($a_row != false)
	}//for($a_ii = 0; $a_ii < $rowsCount; $a_ii++)

	echo '</table>';

	//##버튼 구현
	echo '<br>';
	echo '<div style="text-align: right">';	//버튼들을 오른쪽 정렬
	echo '<input type="submit" value="선택아이템삭제" formaction="Item_Del.php">';
	echo '&nbsp;&nbsp;&nbsp;&nbsp;';
	echo '<input type="submit" value="JSON 파일로 저장" formaction="Item_Update.php">';
	echo '</div>';


	echo '</form>';

	echo '</div>';  //테이블과 버튼들을 묶은 div 종료
	echo '</div>';  //중앙 정렬 종료

	if(isset($_POST['json_message']))
	{
		echo '<br>';
		echo '<div id="json_message" style="color:blue;">' . htmlspecialchars($_POST['json_message']) .'</div><br>';
		echo '<script>
			setTimeout( function()
				    {
					document.getElementById("json_message").style.display="none";
				    }, 
				    5000);
		         </script>';		
		unset($_POST['json_message']);
	}

	mysqli_close($con);
?>