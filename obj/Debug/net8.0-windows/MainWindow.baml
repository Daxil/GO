   M S B A M L   `   `   `  ����=  9GO, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullclr-namespace:GOGO  5   	       GO.MainWindow   5   	   W SPresentationCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5   	   R NSystem.Xaml, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   "localclr-namespace:GO     5   	     � Title$  Игра Го��5   	   $	��600��6   $	��600��6&    � SizeToContent$ WidthAndHeight=�5	   	   .��5
      �  � RowDefinitions 5   
   �� 5      $
��Auto �6   �� 5      $��* �6   �� 5      $
��Auto �6   .+�5   
   �� $��0��6    �� Orientation$ 
Horizontal=�6"   $��Center=�6;   $��5q�6X   .+�5      �� $��Текущий ход:��6   $��0,0,10,0q�6,   A� 5      -   5         A�Name$ CurrentPlayerIndicator��5      $��20��66   $��20��6A   
���   �$��0,0,10,0q�6Z   �� 5      -   5         ��Name$ 
StatusText��5      $��Черные ходят��6,   5      �� 5   
   -   5         ��Name$ GoBoard��5      $��1��6"   
�������$	��500��6F   $	��500��6R   $��Center=�5      $��Center=�6.   5      �� 5    
   $��2��6   $ 
Horizontal=�6"   $��Center=�6;   $��5q�6X   .+�5!      �� -   5         ��Name$ 
PassButton��5!      $��Пас��6)   $��80��67   $��5q�6B   6M   �� 5"      -   5        $ NewGameButton��5"      $��Новая игра��6,   $��80��6A   $��5q�6L   6W   5#      5$      5%      