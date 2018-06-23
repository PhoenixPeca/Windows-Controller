<form method="get">
	input number of terms: <input type="text" name="num1"/>
	<input type="submit"/>
</form>
<br><br><br>

<?php
if(isset($_GET['num1'])) {
	echo $_GET['num1'];

}