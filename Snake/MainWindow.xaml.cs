using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace Snake
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //Поле на котором живет змея
        Entity field;
        // голова змеи
        Head head;
        // вся змея
        List<PositionedEntity> snake;
        // яблоко
        Apple apple;
        //количество очков
        int score;
        //таймер по которому 
        DispatcherTimer moveTimer;

        bool twoLive = false;
        
        //конструктор формы, выполняется при запуске программы
        public MainWindow()
        {
            InitializeComponent();
            
            snake = new List<PositionedEntity>();
            //создаем поле 300х300 пикселей
            field = new Entity(600, 600, "pack://application:,,,/Resources/snake.png");

            //создаем таймер срабатывающий раз в 300 мс
            moveTimer = new DispatcherTimer();
            moveTimer.Interval = new TimeSpan(0, 0, 0, 0, 300);
            moveTimer.Tick += new EventHandler(moveTimer_Tick);
            
        }

        //метод перерисовывающий экран
        private void UpdateField()
        {
            //обновляем положение элементов змеи
            foreach (var p in snake)
            {
                Canvas.SetTop(p.image, p.y);
                Canvas.SetLeft(p.image, p.x);
            }

            //обновляем положение яблока
            Canvas.SetTop(apple.image, apple.y);
            Canvas.SetLeft(apple.image, apple.x);
            
            //обновляем количество очков
            lblScore.Content = String.Format("{0}000", score);
        }

        //обработчик тика таймера. Все движение происходит здесь
        void moveTimer_Tick(object sender, EventArgs e)
        {
            int headx = head.x;
            int heady = head.y;

            switch (head.direction)
            {
                case Head.Direction.DOWN:
                    heady += 40;
                    break;
                case Head.Direction.UP:
                    heady -= 40;
                    break;
                case Head.Direction.LEFT:
                    headx -= 40;
                    break;
                case Head.Direction.RIGHT:
                    headx += 40;
                    break;
            }

            foreach (var p in snake.Where(x => x != head))
            {
                //если координаты головы и какой либо из частей тела совпадают
                if (p.x == headx && p.y == heady)
                {
                    if (score >= 5)
                    {
                        score -= 5;
                        twoLive = true;
                        moveTimer.Stop();
                        return;
                    }
                    //мы проиграли
                    moveTimer.Stop();
                    tbGameOver.Visibility = Visibility.Visible;
                    return;
                }
            }
            if (headx < 40 || headx >= 540 || heady < 40 || heady >= 540)
            {
                if (score >= 5)
                {
                    score -= 5;
                    twoLive = true;
                    moveTimer.Stop();
                    return;
                }
                //мы проиграли
                moveTimer.Stop();
                tbGameOver.Visibility = Visibility.Visible;
                return;
            }

            //в обратном порядке двигаем все элементы змеи
            foreach (var p in Enumerable.Reverse(snake))
            {
                p.move();
            }

            //проверяем, что голова змеи врезалась в яблоко
            if (head.x == apple.x && head.y == apple.y)
            {
                //увеличиваем счет
                score++;
                //двигаем яблоко на новое место
                apple.move();
                // добавляем новый сегмент к змее
                var part = new BodyPart(snake.Last());
                canvas1.Children.Add(part.image);
                snake.Add(part);
            }
            //перерисовываем экран
            UpdateField();
        }

        // Обработчик нажатия на кнопку клавиатуры
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Up:
                    head.direction = Head.Direction.UP;
                    break;
                case Key.Down:
                    head.direction = Head.Direction.DOWN;
                    break;
                case Key.Left:
                    head.direction = Head.Direction.LEFT;
                    break;
                case Key.Right:
                    head.direction = Head.Direction.RIGHT;
                    break;
            }
            if(twoLive &&
                (e.Key == Key.Up || e.Key == Key.Down || e.Key == Key.Right || e.Key == Key.Left))
            {
                twoLive = false;
                moveTimer.Start();
                UpdateField();
            }
        }

        // Обработчик нажатия кнопки "Start"
        private void button1_Click(object sender, RoutedEventArgs e)
        {
            // обнуляем счет
            score = 0;
            // обнуляем змею
            snake.Clear();
            // очищаем канвас
            canvas1.Children.Clear();
            // скрываем надпись "Game Over"
            tbGameOver.Visibility = Visibility.Hidden;
            
            // добавляем поле на канвас
            canvas1.Children.Add(field.image);
            // создаем новое яблоко и добавлем его
            apple = new Apple(snake);
            canvas1.Children.Add(apple.image);
            // создаем голову
            head = new Head();
            snake.Add(head);
            canvas1.Children.Add(head.image);
            
            //запускаем таймер
            moveTimer.Start();
            UpdateField();

        }
        
        public class Entity
        {
            protected int m_width;
            protected int m_height;
            
            Image m_image;
            public Entity(int w, int h, string image)
            {
                m_width = w;
                m_height = h;
                m_image = new Image();
                m_image.Source = (new ImageSourceConverter()).ConvertFromString(image) as ImageSource;
                m_image.Width = w;
                m_image.Height = h;

            }

            public Image image
            {
                get
                {
                    return m_image;
                }
            }
        }

        public class PositionedEntity : Entity
        {
            protected int m_x;
            protected int m_y;
            public PositionedEntity(int x, int y, int w, int h, string image)
                : base(w, h, image)
            {
                m_x = x;
                m_y = y;
            }

            public virtual void move() { }

            public int x
            {
                get
                {
                    return m_x;
                }
                set
                {
                    m_x = value;
                }
            }

            public int y
            {
                get
                {
                    return m_y;
                }
                set
                {
                    m_y = value;
                }
            }
        }

        public class Apple : PositionedEntity
        {
            List<PositionedEntity> m_snake;
            public Apple(List<PositionedEntity> s)
                : base(0, 0, 40, 40, "pack://application:,,,/Resources/fruit.png")
            {
                m_snake = s;
                move();
            }

            public override void move()
            {
                Random rand = new Random();
                do
                {
                    x = rand.Next(13) * 40 + 40;
                    y = rand.Next(13) * 40 + 40;
                    bool overlap = false;
                    foreach (var p in m_snake)
                    {
                        if (p.x == x && p.y == y)
                        {
                            overlap = true;
                            break;
                        }
                    }
                    if (!overlap)
                        break;
                } while (true);

            }
        }

        public class Head : PositionedEntity
        {
            public enum Direction
            {
                RIGHT, DOWN, LEFT, UP, NONE
            };

            Direction m_direction;

            public Direction direction {
                set
                {
                    m_direction = value;
                    RotateTransform rotateTransform = new RotateTransform(90 * (int)value);
                    image.RenderTransform = rotateTransform;
                }
                get { return m_direction; }
            }

            public Head()
                : base(280, 280, 40, 40, "pack://application:,,,/Resources/head.png")
            {
                image.RenderTransformOrigin = new Point(0.5, 0.5);
                m_direction = Direction.NONE;
            }

            public override void move()
            {
                switch (m_direction)
                {
                    case Direction.DOWN:
                        y += 40;
                        break;
                    case Direction.UP:
                        y -= 40;
                        break;
                    case Direction.LEFT:
                        x -= 40;
                        break;
                    case Direction.RIGHT:
                        x += 40;
                        break;
                }

            }
        }

        public class BodyPart : PositionedEntity
        {
            PositionedEntity m_next;
            public BodyPart(PositionedEntity next)
                : base(next.x, next.y, 40, 40, "pack://application:,,,/Resources/body.png")
            {
                m_next = next;
            }

            public override void move()
            {
                x = m_next.x;
                y = m_next.y;
            }
        }
    }
}
