   M S B A M L   `   `   `  ����E  ABTraderWPF, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullW0clr-namespace:BTraderWPF.Windows.LogViewerWindow"BTraderWPF.Windows.LogViewerWindow  5   	   ;"clr-namespace:BTraderWPF.UtilitiesBTraderWPF.Utilities  5   	   (    "StreamingWPF.LogViewerNS.LogViewer   5   	   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   Cmodels0clr-namespace:BTraderWPF.Windows.LogViewerWindow     5   	   8	utilities"clr-namespace:BTraderWPF.Utilities     5   	   -   5          �Name$  
MainWindow��5   4    � Title$ 	LogViewer��6	   $	��862��6   $	��918��6(    � 	Resources 5       �� DataType0   *BTraderWPF.Windows.LogViewerWindow.LogItem �� Path��%�   (��       5	      *, +)�� 6
   "  6   .R�5
      �� ��6   �� $ Message���66   �� $ Color�6Z   5      5      .��5      �  � RowDefinitions 5   
   �� 5      $��* �6   �� 5      $
��Auto �6   .+�5   
   �� -   5         ��Name$ MainListBox��5      $��Stretch=�5      $ �� IsSynchronizedWithCurrentItem$
 TrueY�5      ��.  %� IsVirtualizing . 	 %� VirtualizationMode$	 	Recycling=�5      $��Stretch=�5      *   $BTraderWPF.Utilities.ListBoxBehavior
   ScrollOnNewItem
 5      �� $ 
AutoScroll���5      �� *OutputConcurrentQueue+ � ContextMenu 5      �� 5      .@�5      m� -   5        $
��Copy��5      6-   5      ��5!      �� 5"      .R�5#      $�  . $	 	Recycling=�6C   5$      ��5&      �� 5'       �� 	Resources 5(       �� Color��%W   (��       5*   *   * SystemColors.HighlightBrushKey+)�� 6   $ 
LightGreen��6\   5+      5,      5.      �� 50   
   -   5        $��Open Log File��50      $	��120��6)   $��1��65   $��40��6B   $��Right=�6N   $��10,10,30,10q�51      6&   �� 52   
   -   5         ��Name$ AutoScrollCheckBox��52      $��1��6/   $��Auto Scroll��6b   $
��Left=�6x   $��Center=�6�   $��10,10,10,10q�6�    d� 	IsChecked 6<   �� $ 
AutoScroll�6�   53      54      