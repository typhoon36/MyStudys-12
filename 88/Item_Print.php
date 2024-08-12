<?php

	$ItName = $_POST["ItemName"];
	$ItLevel = $_POST["ItemLevel"];
	$ItAttRate = $_POST["ItemAttRate"];
	$ItPrice = $_POST["ItemPrice"];

	$con = mysqli_connect("localhost", "typhoon", "jun3824!", "typhoon");

	if (!$con) 
	{
    		die("Could not Connect" . mysqli_connect_error());
	}

	if ($ItName == "" || $ItName == "guest")
	 {
 	   echo "guest라는 이름은 리스트만 출력합니다.<br><br>";
	}

	if ($ItName != "" && $ItName != "guest") 
	{
    	$ItName = mysqli_real_escape_string($con, $ItName); // 인젝션 방지
    	

   	 // ## 정보추가
   	 $Result = mysqli_query($con, "INSERT INTO ItmeGrades (ItemName, ItemLevel, ItemAttRate, ItemPrice) VALUES 
       		 ('{$ItName}', '{$ItLevel}', '{$ItAttRate}', '{$ItPrice}');");

   		if ($Result)
        			echo "아이템 추가 성공<br>";
   		 else
       			 echo "Create error.<br>";
	}

	if (isset($_POST['update_message'])) 
	{
    		echo '<div id="update_message" style="color:green;">' . htmlspecialchars($_POST['update_message']) . '</div><br>';
    			echo '<script>
        					setTimeout(function() 
						{
           						 document.getElementById("update_message").style.display="none";		
       						 }, 5000);
    			      </script>';
    		unset($_POST['update_message']);
	}

	if (isset($_POST['del_message'])) 
	{
    		echo '<div id="del_message" style="color:red;">' . htmlspecialchars($_POST['del_message']) . '</div><br>';
   		echo '<script>
        		setTimeout(function() 
			{
           				document.getElementById("del_message").style.display="none";		
        			}, 5000);
   			 </script>';
   		 unset($_POST['del_message']);
	}

	// ## 중앙정렬
	echo '<div style="text-align: center">';

	// ### 테이블 버튼 포함 컨테이너 정렬
	echo '<div style="display:inline-block; text-align: left;">';

	echo '<form method="post" action="Item_Input.html">';	
	echo '<input type="submit" value="아이템 추가">';
	echo '</form>';
	
	//### form 만들기

	echo '<form name = "Item_List" method="post">';
	
	//100개 리스트 가져오기
	$LowList = mysqli_query($con, "SELECT * FROM ItmeGrades ORDER BY idx DESC LIMIT 0, 100"); 
	
	if(!$LowList)
		echo("쿼리 오류 : ". mysqli_error($con));
	echo '<table border="1" width="710" height="10" cellspacing="0">';
	//테이블  height 10을 주지않을경우 여백 생성이 됨.
	
	echo '<tr align="center" bgcolor="blue">';
		echo'<td style="width:60px; height: 30px; color: white">선택</td>';
		echo'<td style="width:80px; color: white">고유번호</td>';
		echo'<td style="width:170px; color: white">아이템이름</td>';
		echo'<td style="color: white">레벨</td>';
		echo'<td style="color: white">공격상승률</td>';
		echo'<td style="color: white">가격</td>';
	echo'</tr>';
	
	$rowsCount = mysqli_num_rows($LowList);
for ($a_ii = 0; $a_ii < $rowsCount; $a_ii++) {
    $a_row = mysqli_fetch_array($LowList);
    if ($a_row !== false) {
        $UniqueId = $a_row["idx"];
        
        echo '<tr align="center">';
            echo '<td style="height: 30px;">'; 
                echo '<input type="radio" name="DelItName" value="'. $UniqueId .'">';
            echo '</td>';
            echo '<td>';
                echo $a_row['idx'];
            echo '</td>';
            
            // htmlspecialchars() 사용자 입력을 안전하게 표기하기 위한 함수
            echo '<td style="padding: 0;">';
                echo '<input type="text" maxlength="12" name="ItemName_' . $UniqueId . '"';
                echo ' value="' . htmlspecialchars($a_row['ItemName']) . '"'; 
                echo ' style="width: 100%; height: 100%; text-align: center; border: 0px;">'; 
            echo '</td>';
            echo '<td style="padding: 0;">';
                echo '<input type="text" maxlength="12" name="ItemLevel_' . $UniqueId . '"';
                echo ' value="' . htmlspecialchars($a_row['ItemLevel']) . '"'; 
                echo ' style="width: 100%; height: 100%; text-align: center; border: 0px;">';
            echo '</td>';
            echo '<td style="padding: 0;">';
                echo '<input type="text" maxlength="12" name="ItemAttRate_' . $UniqueId . '"';
                echo ' value="' . htmlspecialchars($a_row['ItemAttRate']) . '"'; 
                echo ' style="width: 100%; height: 100%; text-align: center; border: 0px;">';
            echo '</td>'; 
            echo '<td style="padding: 0;">';
                echo '<input type="text" maxlength="12" name="ItemPrice_' . $UniqueId . '"';
                echo ' value="' . htmlspecialchars($a_row['ItemPrice']) . '"'; 
                echo ' style="width: 100%; height: 100%; text-align: center; border: 0px;">';
            echo '</td>'; 
        echo '</tr>';
    }
}

	echo '</table>'; 

	//## 버튼
	echo '<br>';
	echo '<div style="text-align: right;">'; 
    	echo '<input type="submit" value="선택 아이템 삭제" formaction="Item_Del.php">';
    	echo '&nbsp;&nbsp;&nbsp;&nbsp;';
    	echo '<input type="submit" value="Json 파일로 저장" formaction="Item_Update.php">';
	echo '</div>';

	echo '</div>';
	echo '</div>'; // 중앙정렬 종료

	if (isset($_POST['json_message'])) 
	{
    		echo '<div id="json_message" style="color:red;">' . htmlspecialchars($_POST['json_message']) . '</div><br>';
   		echo '<script>
        		setTimeout(function() 
			{
           				document.getElementById("json_message").style.display="none";		
        			}, 5000);
   			 </script>';
   		 unset($_POST['json_message']);
	}



	mysqli_close($con);
?>
