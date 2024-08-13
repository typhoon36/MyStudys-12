<?php
	if ( isset($_POST['DelItName']) )  //라디오 버튼 선택 상태 확인
	{
		$UniqueId = $_POST["DelItName"];
		echo "지울 아이템의 고유번호 :  $UniqueId<br>";

		$con = mysqli_connect("localhost", "typhoon", "jun3824!", "typhoon");
    		if(!$con)
        			die('Could not Connect:' . mysqli_connect_error());  

		
		$UniqueId = mysqli_real_escape_string($con, $UniqueId);


		$check = mysqli_query($con, "SELECT idx FROM ItmeGrades WHERE idx='{$UniqueId}'"); 
		$numrows = mysqli_num_rows($check);
		if($numrows == 0)
		{
			mysqli_close($con);
			echo '<form id="postForm" method="post" action="' . $_SERVER['HTTP_REFERER'] . '">';
			echo '<input type="hidden" name="del_message" value="존재하지 않는 Id 입니다.">';
			echo '</form>';
			echo '<script type="text/javascript">document.getElementById("postForm").submit();</script>';
			exit();
		}
		else
		{
			mysqli_query($con, "DELETE FROM ItemGrades WHERE idx='{$UniqueId}'"); //해당 행 삭제하기 명령
		
			//## 테이블을 Json 파일로 저장
			$data = [];
			$JsonMsg = "";
			$result = mysqli_query($con, "SELECT * FROM ItemGrades ORDER BY idx DESC LIMIT 0, 100");
			if($result)
			{
				while($row = mysqli_fetch_assoc($result))
				{
					$data[] =$row;
				}

				$json_data = json_encode(['itemArr' => $data], JSON_PRETTY_PRINT | JSON_UNESCAPED_UNICODE);

				$file_path = 'item_data.json';
				if(file_put_contents($file_path, $json_data))
				{
					$JsonMsg = "JSON 파일이 생성되었습니다. $file_path";
				}
				else
				{
					$JsonMsg = "JSON 파일 생성 오류";
				}	
			}
			else 
			{
				$JsonMsg = "데이터를 가져오는데 오류가 발생했습니다.";
			}
	

			

			mysqli_close($con);
			$deleteMessage = htmlspecialchars($UniqueId) . "고유번호의 아이템을 리스트에서 삭제하고 Json 파일로 저장 했습니다.";
			echo '<form id="postForm" method="post" action="' . $_SERVER['HTTP_REFERER'] . '">';
			echo '<input type="hidden" name="del_message" value="' . $deleteMessage . '">';
			echo '<input type="hidden" name="json_message" value="' . $JsonMsg . '">';
			echo '</form>';
			echo '<script type="text/javascript">document.getElementById("postForm").submit();</script>';
			exit();			

		}

	}
	else 
	{
		echo '<form id="postForm" method="post" action="' . $_SERVER['HTTP_REFERER'] . '">';
		echo '<input type="hidden" name="del_message" value="체크된 라디오 버튼이 없습니다.">';
		echo '</form>';
		echo '<script type="text/javascript">document.getElementById("postForm").submit();</script>';
		exit();
	}
?>