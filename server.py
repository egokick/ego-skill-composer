from flask import Flask, request, jsonify, send_from_directory
import json

app = Flask(__name__)

@app.route('/save_json', methods=['POST'])
def save_json():
    data = request.get_json()
    with open('skills.json', 'w') as f:
        f.write(json.dumps(data, indent=4))
    return jsonify({"message": "JSON data saved successfully!"})

@app.route('/skills.json', methods=['GET'])
def get_skills():
    return send_from_directory('.', 'skills.json')

@app.route('/')
def index():
    return send_from_directory('.', 'index.html')

if __name__ == '__main__':
    app.run(debug=True)
