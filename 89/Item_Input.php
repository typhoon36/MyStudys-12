<html>
	<head>
		<title>아이템 관리 프로그램</title>
	</head>

	<body>
		<div style="text-align:center">
		<h2>▶ 아이템 입력 폼</h2>

		<!-- <form method="post" action="Item_Print.php"> -->
		<form name="Input_Item" method="post">
			<input type="hidden" name="title" value="아이템 입력 양식">

			<table border="1" width="640" cellspacing="1" cellpadding="4" 
				style="margin-left: auto; margin-right: auto;">

				<tr>
					<td align="right"> * 아이템 이름 : </td>
					<td><input type="text" maxlength="12" name="ItemName" value="guest"></td>
				</tr>

				<tr>
					<td align="right"> * 레벨 (1 ~ 30) : </td>
					<td><input type="text" maxlength="12" name="ItemLevel" value="0"></td>
				</tr>

				<tr>
					<td align="right"> * 공격상승률 (0.01 ~ 1.0) : </td>
					<td><input type="text" maxlength="12" name="ItemAttRate" value="0"></td>
				</tr>

				<tr>
					<td align="right"> * 가격 (500 ~ 10000) : </td>
					<td><input type="text" maxlength="12" name="ItemPrice" value="0"></td>
				</tr>
			</table>
			<br>
			<table border="0" width="640" style="margin-left: auto; margin-right: auto;">
				<tr>
					<td align="center">
						<!-- <input type="submit" value="확인"> &nbsp; &nbsp; -->
						<input type="submit" value="확인" name="AddItem"> &nbsp; &nbsp;
						<input type="reset" value="다시작성">
					</td>
				</tr>
			</table>

		</form>
		</div>
		
	     <?php
		if( isset($_POST['AddItem']) )
		{
			$ItName = $_POST["ItemName"];   //아이템 이름
			$ItLevel = $_POST["ItemLevel"]; //아이템 레벨
			$ItAttRate = $_POST["ItemAttRate"]; //아이템 공격상승률
			$ItPrice = $_POST["ItemPrice"];	//아이템 가격	

			//echo '<br>';
			//echo "아이템 이름 : " . $ItName . "<br>";
			//echo "아이템 레벨 : " . $ItLevel . "<br>";
			//echo "아이템 공격상승률 : " . $ItAttRate . "<br>";	
			//echo "아이템 가격 : " . $ItPrice . "<br>";	

			$My_Input_Msg = "";			
			if( !empty($ItName) && $ItName != "guest")
			{
				$con = mysqli_connect("localhost", "typhoon", "jun3824!", "typhoon");
				if(!$con)
				{
					$My_Input_Msg = "Could not Connect " . mysqli_connect_error();
					
				}
				else
				{
					$ItName = mysqli_real_escape_string($con, $ItName);  //함수를 사용하여 SQL 인젝션(해킹) 방지
						
					//정보를 추가해 주는 쿼리문
					$Result = mysqli_query($con, "INSERT INTO ItemGrades (ItemName, ItemLevel, ItemAttRate, ItemPrice) VALUES 
							('{$ItName}', '{$ItLevel}', '{$ItAttRate}', '{$ItPrice}');");
					if($Result)
						$My_Input_Msg = "아이템 추가 성공";
					else
						$My_Input_Msg = "아이템 추가 실패";

					mysqli_close($con);
				}
             			}//if($ItName != "" && $ItName != "guest")

			//다음 페이지로 넘어가기
			echo '<form id="postForm" method="post" action="Item_Print.php">';
			echo '<input type="hidden" name="Item_Input_Msg" value="' .htmlspecialchars($My_Input_Msg) . '">';
			echo '</form>';
			echo '<script type="text/javascript">document.getElementById("postForm").submit();</script>'; 
			exit();		
		}//if( isset($_POST['AddItem']) )
	     ?>

	</body>

</html>