ArrayList<Point> points = new ArrayList<Point>();
color[] colors = new color[] { color(255, 50, 0), color(150, 50, 50), color(0, 50, 255), color(0, 255, 0), color(100, 100, 100), color(255, 0, 255)};

void setup() {
  size(600, 600);
  background(255);
  
  readClusterPoints("dbscan.json", points);
  for (int i = 0; i < points.size(); i++) {
    points.get(i).show();
  }
}
  

int readClusterPoints (String filename, ArrayList<Point> toAdd) {
  JSONArray values = loadJSONArray(filename);
  for (int i = 0; i < values.size(); i++) {
    JSONObject jp = values.getJSONObject(i);
    toAdd.add(new Point(jp.getFloat("x"), jp.getFloat("y"), jp.getInt("c")));
  }
  return values.size();
}


class Point {
  float x, y;
  int c;
  public Point(float x, float y, int c) {
    this.x = x;
    this.y = y;
    this.c = c;
  }
  
  public void show() {
    fill(colors[c]);
    ellipse(x, y, 4, 4);
  }
}
