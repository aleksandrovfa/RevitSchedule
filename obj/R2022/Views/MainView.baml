   M S B A M L   `   `   `  ���� M  IRevitSchedule_2022, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null-clr-namespace:RevitScheduleRevitSchedule      RevitSchedule.MainView   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     @d2http://schemas.microsoft.com/expression/blend/2008     -localclr-namespace:RevitSchedule     $	��450��$	��230��  � Title$  Расчет ТРН��.���  � RowDefinitions �� $��35 ��� �� $��40 �.+��� $6��0Выберите щиты для расчета:��$��5q�$��0���� $
��Auto=�$
��Auto=�$��1��$��5q�.����  �� SelectionMode$ Multiple=� �� DisplayMemberPath$
 Name������ *ElectrEquipAll+����  �� 
TargetType" ��.���  �� Property �@��
IsSelected �� Value �� *
IsSelected+��  � �� ��  �� 
TargetType" ��.R���  ��Name$ Bd��#��z��#��z��#��z��	 �� Padding#	 z��
 J� SnapsToDevicePixels
 . ._��� #��z��#
 z
 #��z�� �� Triggers P�  P� Property �@J�IsMouseOver P� Value . . ���  � �� �	#1F26A0DA��  � �� �	#a826A0DaP�  �@��
IsSelected . . ���  � �� �	#3D26A0DA��  � �� �	#FF26A0DA�� $��Рассчитать��$��5q�$��2������ *SaveCommand+