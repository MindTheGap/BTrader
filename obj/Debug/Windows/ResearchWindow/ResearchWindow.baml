   M S B A M L   `   `   `  ����  
BTraderWPF6    0BTraderWPF.Windows.ResearchWindow.ResearchWindow   5   	   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   -   5          � Title$  ResearchWindow��5   	   $	��500��6    $	��800��6-   .��5      � 
����ح� � RowDefinitions 5   
   �� 5      $
��Auto �6   �� 5	      $
��Auto �6   �� 5
      $��* �6   �� 5      $
��Auto �6   .+�5   
   � $��0��6   
����ح�.+�5      o� -   5         o�Name$ MainMenu��5      $
��Auto��6%   
����ح�$��30��6I   $��12(�6U   $	��Top=�6c   $
��Left=�6x   .@�5      m� $��_File��6   
����ح�.@�5      m� $��_Reload Config��6    m� Command 67   ��  �� Path$ ReloadConfigCommand�m� 5      $��_Exit��6    6.   �� $ ExitCommand�5      m� 5      $��_Tools��6   $
��Left=�6+   
����ح�.@�5      m� $��_Show Log Viewer��6    69   �� $ ShowLogViewerCommand�5      5      5      � 5   
   $��1��6    � ColumnDefinitions 5      �� 5      $��* �6#   �� 5      $��* �6#   .+�5      �� $��0��6    �� Orientation$ 
Horizontal=�6)   $��Center=�6B   .+�5      �� $��Center=�6   $��Start Date:��67   (  "System.Windows.Controls.DatePicker  5      $��Center=�6      SelectedDate 68   �� $ 	StartDate�   DisplayDate 6`   �� $ 	StartDate�5       �� 5!      $��1��6   $ 
Horizontal=�6)   .+�5"      �� $��Center=�6   $��	End Date:��67     5#      $��Center=�6    68   �� $ 	StartDate� 6`   �� $ 	StartDate�5$      5%      &   System.Windows.Controls.DataGrid  5'   
   -   5        	  Name$	 MainDataGrid��5'       $��2��6   
   AutoGenerateColumns
 .  ��5)      �� $ Steps�   Columns 5*      0  *System.Windows.Controls.DataGridTextColumn  5+      ,  &System.Windows.Controls.DataGridColumn   Header$ 	Step Name��6%      Width5  /System.Windows.Controls.DataGridLengthConverter$
 Auto 6W   1  +System.Windows.Controls.DataGridBoundColumn   Binding 69   �� $
 Name�   HeaderStyle 5,      �� 5-       �� 
TargetType=  7System.Windows.Controls.Primitives.DataGridColumnHeader"  6    .�5.      ��  �� Property � �� �� Value$ Center=�6K   5/      51      4  .System.Windows.Controls.DataGridTemplateColumn  52      $	 250 6)   $ Progress��65      CellTemplate 53      �� 54      .R�55      � .+�56   "   �  �Name$ StepProgressBar��6.   
������ � Value 6G   �� $ Progress�6g   �� 57   "   $��Center=�6|   $��Center=�6�   ��6,   ��  �� ElementName$ StepProgressBar��$ Value� �� StringFormat$ {0:0}%��6�   58      59       5;      �� 5<      "  6    .�5=      ��  � ��$ Center=�6K   5>      5@        5A      $ 	Arguments��6%   $ * 6[    68   �� $ 	Arguments� 5B      �� 5C      "  6    .�5D      ��  � ��$ Center=�6K   5E      5G      5I      �� 5K   
   $��3��6   $��80��6u   $��Right=�6�   $��Center=�6�   $��Center=�6�   
����ح�{�6   �� $ StartButtonEnabled���6L   �� $ StartButtonText���5L      �� $ StartButtonCommand�6=   5M      5N      