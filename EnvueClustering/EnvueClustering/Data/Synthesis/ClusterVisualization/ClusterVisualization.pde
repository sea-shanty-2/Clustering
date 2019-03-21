
ArrayList<Point> points = new ArrayList<Point>();

void setup() {
  size(600, 600);
  
  points.add(new Point(250, 300, 5)); 
  points.add(new Point(300, 250, 5));
  points.add(new Point(300, 350, 5));
  points.add(new Point(350, 300, 5));

  
  //readPoints("test");
  //noFill();
  fill(50);
  for (int i = 0; i < points.size(); i++) {
    points.get(i).show();
  }
  
  fill(255, 0, 0);
  Point p = new Point(300, 300, 5);
  p.show();
}



void readPoints(String fileName) {
  String[] lines = loadStrings(fileName);
  for (int i = 0; i < lines.length; i++) {
    String[] attrs = split(lines[i], ' ');
    float x = float(attrs[0]);
    float y = float(attrs[1]);
    float r = float(attrs[2]);
    points.add(new Point(x, y, r));
  }
}

class Point {
  float x, y, r;
  public Point(float x, float y, float r) {
    this.x = x;
    this.y = y;
    this.r = r;
  }
  
  public void show() {
    ellipse(x, y, r * 2, r * 2);
  }
}
