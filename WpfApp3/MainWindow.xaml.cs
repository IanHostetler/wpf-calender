using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Data.SqlClient;
using Microsoft.VisualBasic.ApplicationServices;

namespace WpfApp3
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>

    public partial class MainWindow : Window
    {

        public MainWindow()
        {
            InitializeComponent();
            currentDate = DateTime.Now;

            UpdateMonthYearText();
            PopulateCalendarGrid();
            FetchUserDataForDay(currentDate); // Call FetchUserDataForDay with currentDate
            PopulateMonthComboBox();
        }

        private void PopulateCalendarGrid()
        {
            CalendarDays.Children.Clear(); // Clear existing children

            // Get the number of days in the current month
            int daysInMonth = DateTime.DaysInMonth(currentDate.Year, currentDate.Month);
            DateTime firstDayOfMonth = new DateTime(currentDate.Year, currentDate.Month, 1);
            DayOfWeek startingDayOfWeek = firstDayOfMonth.DayOfWeek;

            // Start populating the grid from the starting day of the week
            int currentColumn = (int)startingDayOfWeek;
            int currentRow = 0; // Start from the second row (below the days of the week)

            for (int day = 1; day <= daysInMonth; day++)
            {
                // Create a TextBlock to display the day number
                TextBlock dayTextBlock = new TextBlock();
                dayTextBlock.Text = day.ToString();
                dayTextBlock.FontSize = 18;
                dayTextBlock.Foreground = Brushes.Black;
                dayTextBlock.HorizontalAlignment = HorizontalAlignment.Right;
                dayTextBlock.VerticalAlignment = VerticalAlignment.Top;
                dayTextBlock.TextAlignment = TextAlignment.Right;
                dayTextBlock.Margin = new Thickness(1,1,4,1);

                // Fetch user data for the current day from the database
                DateTime currentDateForDay = new DateTime(currentDate.Year, currentDate.Month, day);
                string userData = FetchUserDataForDay(currentDateForDay);

                // Check if there is SQL data for the current day
                if (!string.IsNullOrEmpty(userData))
                {
                    // Split the SQL data into individual entries
                    string[] userEntries = userData.Split('\n');

                    // Create a StackPanel to hold the day number and user data vertically
                    StackPanel stackPanel = new StackPanel();

                    // Add the dayTextBlock to the StackPanel
                    stackPanel.Children.Add(dayTextBlock);
                    // Somethin
                    int greyBoxCount = 0;

                    // Create a Border element with grey background for each user entry
                    foreach (string entry in userEntries)
                    {
                        string[] userInfo = entry.Split(','); // Assuming the entry format is "UserId: {userId}, FirstName: {firstName}, LastName: {lastName}"
                        string workorderId = "";
                        string description = "";
                        string UserId = "";
                        string SiteId = "";
                        int status = 0; // Initialize status
                        foreach (string info in userInfo)
                        {
                            if (info.Trim().StartsWith("WorkOrderId:"))
                            {
                                workorderId = info.Split(':')[1].Trim();
                            }
                            else if (info.Trim().StartsWith("Description:"))
                            {
                                description = info.Split(':')[1].Trim();
                            }
                            else if (info.Trim().StartsWith("UserId:"))
                            {
                                UserId = info.Split(':')[1].Trim();
                            }
                            else if (info.Trim().StartsWith("SiteId:"))
                            {
                                SiteId = info.Split(':')[1].Trim();
                            }
                            else if (info.Trim().StartsWith("Status:"))
                            {
                                status = int.Parse(info.Split(':')[1].Trim());
                            }

                        }
                        // Create a TextBlock to display the user data
                        TextBlock userDataTextBlock = new TextBlock();
                        userDataTextBlock.Text = $"WorkOrder #{workorderId} {description} ";
                        userDataTextBlock.Foreground = Brushes.Black;
                        userDataTextBlock.HorizontalAlignment = HorizontalAlignment.Stretch; // Stretch horizontally
                        userDataTextBlock.VerticalAlignment = VerticalAlignment.Center; // Center vertically
                        userDataTextBlock.TextAlignment = TextAlignment.Left; // Align text to the left

                        // Fetching first name and last name

                        

                        // Create tooltip content
                        StringBuilder tooltipContent = new StringBuilder();
                        tooltipContent.AppendLine($"WorkOrder #{workorderId} \nDesc: {description} \nUserID: {UserId} \nSiteID:{SiteId}");

                        // Create a tooltip for the userDataBorder
                        ToolTip toolTip = new ToolTip();
                        toolTip.Content = tooltipContent.ToString();

                        // Create a Border element to wrap the userDataTextBlock
                        Border userDataBorder = new Border();
                        userDataBorder.BorderThickness = new Thickness(0);
                        userDataBorder.CornerRadius = new CornerRadius(4);
                        userDataBorder.Padding = new Thickness(3, 1, 1, 1);
                        userDataBorder.Margin = new Thickness(1);
                        userDataBorder.Child = userDataTextBlock;
                        userDataBorder.ToolTip = toolTip; // Set the tooltip for the userDataBorder
                        SolidColorBrush backgroundColor;
                        switch (status)
                        {
                            case 1:
                                backgroundColor = Brushes.Green; // Change to desired color for status 1
                                break;
                            case 2:
                                backgroundColor = Brushes.Yellow; // Change to desired color for status 2
                                break;
                            case 3:
                                backgroundColor = Brushes.Red; // Change to desired color for status 3
                                break;
                            default:
                                backgroundColor = Brushes.Gray; // Default color
                                break;
                        }
                        userDataBorder.Background = backgroundColor;
                        // Add the userDataBorder to the StackPanel
                        stackPanel.Children.Add(userDataBorder);

                        // Increment the greyBoxCount
                        greyBoxCount++;
                    }

                    // Subtract one grey box from the StackPanel if there are more than one
                    if (greyBoxCount > 1)
                    {
                        // Remove the last grey box added
                        stackPanel.Children.RemoveAt(stackPanel.Children.Count - 1);
                    }

                    // Create a Border element to wrap the StackPanel
                    Border border = new Border();
                    border.BorderBrush = Brushes.Black; // Set the border color
                    border.BorderThickness = new Thickness(1); // Set the border thickness
                    border.Child = stackPanel; // Add the StackPanel as a child of the Border
                    border.Margin = new Thickness(-.5);


                    // Add the Border to the grid
                    Grid.SetRow(border, currentRow);
                    Grid.SetColumn(border, currentColumn);
                    border.HorizontalAlignment = HorizontalAlignment.Stretch;

                    // Set the vertical alignment of the Border to stretch if it's not in the first row
                    if (currentRow > 0)
                    {
                        border.VerticalAlignment = VerticalAlignment.Stretch;
                    }

                    CalendarDays.Children.Add(border);
                }
                else
                {
                    // If there is no SQL data, simply add the dayTextBlock without a grey box
                    // Create a Border element to wrap the dayTextBlock
                    Border border = new Border();
                    border.BorderBrush = Brushes.Black; // Set the border color
                    border.BorderThickness = new Thickness(1); // Set the border thickness
                    border.Padding = new Thickness(0); // Add padding to create space between border and content
                    border.Child = dayTextBlock; // Add the dayTextBlock as a child of the Border
                    border.Margin = new Thickness(-.5);

                    // Add the Border to the grid
                    Grid.SetRow(border, currentRow);
                    Grid.SetColumn(border, currentColumn);
                    border.HorizontalAlignment = HorizontalAlignment.Stretch;

                    // Set the vertical alignment of the Border to stretch if it's not in the first row
                    if (currentRow > 0)
                    {
                        border.VerticalAlignment = VerticalAlignment.Stretch;
                    }

                    CalendarDays.Children.Add(border);
                }

                currentColumn++;
                if (currentColumn > 6) // If we reached the last column
                {
                    currentColumn = 0; // Reset to the first column
                    currentRow++; // Move to the next row
                }
            }
        }

        private string FetchUserDataForDay(DateTime targetDate)
        {
            try
            {
                // Create a connection string with SQL Server Authentication
                SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
                builder.DataSource = "";
                builder.InitialCatalog = "";
                builder.UserID = "";
                builder.Password = "!";
                builder.TrustServerCertificate = true;
                builder.IntegratedSecurity = false; // Turn off Windows Authentication

                // Show a success message
                //MessageBox.Show("Connection succeeded!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                // Execute the query to fetch user data for the given date
                using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
                {
                    connection.Open();

                    // Create a SqlCommand object for the stored procedure
                    using (SqlCommand command = new SqlCommand("GetWorkOrderInfo", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        // Add the parameter for the stored procedure
                        command.Parameters.AddWithValue("@StartDate", targetDate);

                        // Execute the stored procedure
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            StringBuilder workOrderInfoBuilder = new StringBuilder();
                            while (reader.Read())
                            {
                                // Assuming the columns in the result set match the properties in your C# class
                                int workOrderId = reader.GetInt32(0);
                                int userId = reader.GetInt32(1);
                                int siteId = reader.GetInt32(2);
                                int workOrderTypeId = reader.GetInt32(3);
                                string description = reader.GetString(4);
                                int priorityId = reader.GetInt32(5);
                                int status = reader.GetInt32(6); // Assuming Status is an integer in the database
                                DateTime startDate = reader.GetDateTime(7);
                                
                                string purchaseOrderIds = reader.GetString(9);
                                string workOrderTaskIds = reader.GetString(10);
                                string assetIds = reader.GetString(11);


                                // Append the work order info to the StringBuilder
                                workOrderInfoBuilder.AppendLine($"WorkOrderId: {workOrderId}, UserId: {userId}, SiteId: {siteId}, WorkOrderTypeId: {workOrderTypeId}, Description: {description}, PriorityId: {priorityId}, Status: {status}, StartDate: {startDate}, PurchaseOrderIds: {purchaseOrderIds}, WorkOrderTaskIds: {workOrderTaskIds}, AssetIds: {assetIds}");
                            }
                            // Return the concatenated work order info
                            return workOrderInfoBuilder.ToString();
                        }
                    }
                }
            
            }
            catch (SqlException e)
            {
                // Show an error message
                MessageBox.Show("Error fetching user data: " + e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                // Log or handle the exception
                Debug.WriteLine("Error fetching user data: " + e.Message);
            }

            return ""; // Return empty string if no user data found
        }


        private DateTime currentDate;
        private void PreviousMonthButton_Click(object sender, RoutedEventArgs e)
        {
            currentDate = currentDate.AddMonths(-1);
            UpdateMonthYearText();
            PopulateCalendarGrid();
        }

        private void NextMonthButton_Click(object sender, RoutedEventArgs e)
        {
            currentDate = currentDate.AddMonths(1);
            UpdateMonthYearText();
            PopulateCalendarGrid();
        }

        private void UpdateMonthYearText()
        {
            MonthYear.Text = currentDate.ToString("MMMM yyyy");
        }
        private void ApplyDarkMode()
        {
            // Define dark mode colors
            Color darkBackground = Color.FromArgb(0xFF, 0x1E, 0x1E, 0x1E);
            Color darkForeground = Colors.White;

            // Set dark mode colors
            this.Background = new SolidColorBrush(darkBackground);

            foreach (var child in WeekDays.Children)
            {
                if (child is TextBlock textBlock)
                {
                    textBlock.Foreground = new SolidColorBrush(darkForeground);
                }
            }

            foreach (var child in CalendarDays.Children)
            {
                if (child is TextBlock textBlock)
                {
                    textBlock.Foreground = new SolidColorBrush(darkForeground);
                }
            }
        }
        private void MonthComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Get the selected month
            string selectedMonth = MonthComboBox.SelectedItem as string;
            if (!string.IsNullOrEmpty(selectedMonth))
            {
                // Get the index of the selected month
                int monthIndex = Array.IndexOf(CultureInfo.CurrentCulture.DateTimeFormat.MonthNames, selectedMonth);
                if (monthIndex != -1)
                {
                    // Create a DateTime object for the selected month
                    DateTime selectedDate = new DateTime(currentDate.Year, monthIndex + 1, 1); // +1 because monthIndex is zero-based
                                                                                               // Navigate to the selected date
                    currentDate = selectedDate;
                    UpdateMonthYearText();
                    PopulateCalendarGrid();
                }
            }
        }

        // Method to populate the ComboBox with all the months
        private void PopulateMonthComboBox()
        {
            // Clear existing items
            MonthComboBox.Items.Clear();

            // Populate the ComboBox with all the months
            for (int i = 1; i <= 12; i++)
            {
                MonthComboBox.Items.Add(CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(i));
            }
        }




    }
}
