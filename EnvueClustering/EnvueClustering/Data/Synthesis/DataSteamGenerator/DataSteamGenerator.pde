boolean isMousePressed = false;
int globalCount = 0;
ArrayList<Point> points = new ArrayList<Point>();

void setup() {
  size(600, 600);
}

void draw() {
  background(255);
  for (int i = 0; i < points.size(); i++) {
    points.get(i).show();
  }
  
  if (isMousePressed) {
    spawnPoints();
  }
}

void spawnPoints() {
  if (mouseButton == LEFT) {
    float x = random(-50, 50);
    float yLim = sqrt(pow(50, 2) - pow(x, 2));
    float y = random(-yLim, yLim);
    Point p = new Point(mouseX + x, mouseY + y, globalCount);
    points.add(p);
    globalCount++;
  }
}

void saveData() {
  JSONArray arr = new JSONArray();
  for (int i = 0; i < points.size(); i++) {
    JSONObject p = new JSONObject();
    Point rp = points.get(i);
    p.setFloat("x", rp.x);
    p.setFloat("y", rp.y);
    p.setFloat("timeStamp", rp.spawnTime);
    arr.setJSONObject(i, p);
  }

  saveJSONArray(arr, "data.synthetic");
}

void keyPressed() {
  if (keyCode == ENTER) {
    saveData();
    exit();
  }
}

void mousePressed() {
  isMousePressed = true;
}

void mouseReleased() {
  isMousePressed = false;
}

class Point {
  float x, y;
  int spawnTime;
  
  public Point(float x, float y, int spawnTime) {
    this.x = x;
    this.y = y;
    this.spawnTime = spawnTime;
  }
  
  public void show() {
    ellipse(x, y, 5, 5);
  }
}
