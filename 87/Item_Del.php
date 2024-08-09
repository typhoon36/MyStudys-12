<?php
	if ( isset($_POST['DelItName']) )  //라디오 버튼 선택 상태 확인
	{
		$UniqueId = $_POST["DelItName"];
		echo "지울 아이템의 고유번호 :  $UniqueId<br>";

		$con = mysqli_connect("localhost", "typhoon", "jun3824!", "typhoon");
    		if(!$con)
        			die('Could not Connect:' . mysqli_connect_error());  

		$check = mysqli_query($con, "SELECT idx FROM ItmeGrades WHERE idx='{$UniqueId}'"); 
		
		$numrows = mysqli_num_rows($check);
		if($numrows == 0)
		{
			mysqli_close($con);
			die("존재하지 않는 ID입니다.\n");
		}
		else
		{
			mysqli_query($con, "DELETE FROM ItmeGrades WHERE idx='{$UniqueId}'"); //해당 행 삭제하기 명령
 			echo "<br><br>";
			echo "$UniqueId 고유번호의 아이템을 리스트에서 삭제 했습니다.";
		}

		mysqli_close($con);
	}
	else 
	{
    		echo "체크된 레디오 버튼이 없습니다.";
	}
?>