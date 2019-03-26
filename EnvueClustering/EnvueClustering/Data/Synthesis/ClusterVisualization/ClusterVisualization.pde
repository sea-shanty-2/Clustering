
ArrayList<Point> points = new ArrayList<Point>();
ArrayList<Point> pcmcs = new ArrayList<Point>();
ArrayList<Point> ocmcs = new ArrayList<Point>();
ArrayList<Point> pcmcPoints = new ArrayList<Point>();
ArrayList<Point> ocmcPoints = new ArrayList<Point>();

void setup() {
  size(600, 600);
  background(255);
  
  // points.add(new Point(250, 300, 5)); 
  // points.add(new Point(300, 250, 5));
  // points.add(new Point(300, 350, 5));
  // points.add(new Point(350, 300, 5));

  readRawPoints("data.synthetic.json", points);
  readPoints("ocmcs.json", ocmcs);
  readPoints("ocmcPoints.json", ocmcPoints);
  readPoints("pcmcs.json", pcmcs);
  readPoints("pcmcPoints.json", pcmcPoints);
  noStroke();
  fill(150, 150, 150);
  for (int i = 0; i < points.size(); i++) {
    points.get(i).show();
  }
  
  stroke(0);
  fill(255, 0, 0);
  for (int i = 0; i < ocmcPoints.size(); i++) {
    ocmcPoints.get(i).show();
  }
  
  fill(0, 200, 0);
  for (int i = 0; i < pcmcPoints.size(); i++) {
    pcmcPoints.get(i).show();
  }
  
  noFill();
  stroke(255, 0, 0);
  for (int i = 0; i < ocmcs.size(); i++) {
    ocmcs.get(i).show();
  }
  
  stroke(0, 200, 0);
    for (int i = 0; i < pcmcs.size(); i++) {
      pcmcs.get(i).show();
  }
}

int readRawPoints (String filename, ArrayList<Point> toAdd) {
  JSONArray values = loadJSONArray(filename);
  for (int i = 0; i < values.size(); i++) {
    JSONObject jp = values.getJSONObject(i);
    toAdd.add(new Point(jp.getFloat("x"), jp.getFloat("y"), 2));
  }
  return values.size();
}

int readPoints(String filename, ArrayList<Point> toAdd) {
  JSONArray values = loadJSONArray(filename);
  for (int i = 0; i < values.size(); i++) {
    JSONObject jp = values.getJSONObject(i);
    toAdd.add(new Point(jp.getFloat("X"), jp.getFloat("Y"), jp.getFloat("Radius")));
  }
  return values.size();
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
